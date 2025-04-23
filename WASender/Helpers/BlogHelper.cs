using System.Text.Json;
using WASender.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace WASender.Helpers
{
    public class BlogHelper
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BlogHelper(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task CreateAsync(IFormCollection request)
        {
            var title = request["title"].ToString();
            if (string.IsNullOrWhiteSpace(title))
                throw new Exception("Title is required.");

            var post = new Post
            {
                Title = title,
                Slug = Slugify(title),
                Type = "blog",
                Featured = request.ContainsKey("featured") ? 1 : 0,
                Status = request.ContainsKey("status") ? 1 : 0,
                Lang = request["language"].FirstOrDefault() ?? "en",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync(); // Save here to get the post.Id ✅

            // --- Categories ---
            if (request.ContainsKey("categories"))
            {
                foreach (var idStr in request["categories"])
                {
                    if (ulong.TryParse(idStr, out var categoryId))
                    {
                        _context.Postcategories.Add(new Postcategory
                        {
                            PostId = post.Id,
                            CategoryId = categoryId
                        });
                    }
                }
            }

            // --- Preview Image Upload ---
            if (request.Files.Any(f => f.Name == "preview"))
            {
                var previewFile = request.Files["preview"];
                var previewPath = await UploaderHelper.SaveFileAsync(previewFile, _env);
                _context.Postmetas.Add(new Postmeta
                {
                    PostId = post.Id,
                    Key = "preview",
                    Value = previewPath
                });
            }

            // --- Short and Long Description ---
            _context.Postmetas.Add(new Postmeta
            {
                PostId = post.Id,
                Key = "short_description",
                Value = request["short_description"]
            });
            _context.Postmetas.Add(new Postmeta
            {
                PostId = post.Id,
                Key = "main_description",
                Value = request["main_description"]
            });

            // --- SEO Meta ---
            var seo = new Dictionary<string, string>
            {
                ["title"] = request["meta_title"],
                ["description"] = request["meta_description"],
                ["tags"] = request["meta_tags"]
            };

            if (request.Files.Any(f => f.Name == "meta_image"))
            {
                var metaImageFile = request.Files["meta_image"];
                var metaImagePath = await UploaderHelper.SaveFileAsync(metaImageFile, _env);
                seo["image"] = metaImagePath;
            }

            _context.Postmetas.Add(new Postmeta
            {
                PostId = post.Id,
                Key = "seo",
                Value = JsonSerializer.Serialize(seo)
            });

            await _context.SaveChangesAsync();
        }

        private string Slugify(string input)
        {
            return input.Trim().ToLower().Replace(" ", "-");
        }

        public async Task UpdateAsync(IFormCollection request, ulong id)
        {
            var post = await _context.Posts
                .Include(p => p.Postmetas)
                .FirstOrDefaultAsync(p => p.Id == id && p.Type == "blog");

            if (post == null) throw new Exception("Post not found");

            post.Title = request["title"];
            post.Slug = Slugify(request["title"]);
            post.Featured = request.ContainsKey("featured") ? 1 : 0;
            post.Status = request.ContainsKey("status") ? 1 : 0;
            post.Lang = request["language"].FirstOrDefault() ?? "en";
            post.UpdatedAt = DateTime.UtcNow;

            // Update preview image
            var previewMeta = post.Postmetas.FirstOrDefault(m => m.Key == "preview");
            if (request.Files.Any(f => f.Name == "preview"))
            {
                if (previewMeta != null)
                    UploaderHelper.RemoveFile(_env.WebRootPath + previewMeta.Value);

                var newPreview = await UploaderHelper.SaveFileAsync(request.Files["preview"], _env);
                if (previewMeta != null)
                    previewMeta.Value = newPreview;
                else
                    AddMeta(post.Id, "preview", newPreview);
            }

            UpdateOrCreateMeta(post, "short_description", request["short_description"]);
            UpdateOrCreateMeta(post, "main_description", request["main_description"]);

            var seoObj = new Dictionary<string, string>
            {
                ["title"] = request["meta_title"],
                ["description"] = request["meta_description"],
                ["tags"] = request["meta_tags"]
            };

            if (request.Files.Any(f => f.Name == "meta_image"))
            {
                var newImage = await UploaderHelper.SaveFileAsync(request.Files["meta_image"], _env);

                var seoMeta = post.Postmetas.FirstOrDefault(m => m.Key == "seo");
                if (seoMeta != null)
                {
                    var existingSeo = JsonSerializer.Deserialize<Dictionary<string, string>>(seoMeta.Value);
                    if (existingSeo != null && existingSeo.TryGetValue("image", out var oldImg))
                    {
                        UploaderHelper.RemoveFile(_env.WebRootPath + oldImg);
                    }
                }

                seoObj["image"] = newImage;
            }

            var seoSerialized = JsonSerializer.Serialize(seoObj);
            var existingSeoMeta = post.Postmetas.FirstOrDefault(m => m.Key == "seo");

            if (existingSeoMeta != null)
                existingSeoMeta.Value = seoSerialized;
            else
                AddMeta(post.Id, "seo", seoSerialized);

            await _context.SaveChangesAsync();
        }

        private void AddMeta(ulong postId, string key, string? value)
        {
            _context.Postmetas.Add(new Postmeta
            {
                PostId = postId,
                Key = key,
                Value = value ?? ""
            });
        }
        private void AddPostMeta(ulong postId, string key, string? value)
        {
            _context.Postmetas.Add(new Postmeta
            {
                PostId = postId,
                Key = key,
                Value = value ?? ""
            });
        }

        private void UpdateOrCreateMeta(Post post, string key, string value)
        {
            var meta = post.Postmetas.FirstOrDefault(m => m.Key == key);
            if (meta != null)
            {
                meta.Value = value;
            }
            else
            {
                AddMeta(post.Id, key, value);
            }
        }

       
    }
}
