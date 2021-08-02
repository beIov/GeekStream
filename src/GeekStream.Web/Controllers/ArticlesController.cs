﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using GeekStream.Core.Entities;
using GeekStream.Core.Services;
using GeekStream.Core.ViewModels;


namespace GeekStream.Web.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly ArticleService _articleService;
        private readonly CategoryService _categoryService;
        private readonly CommentService _commentService;

        public ArticlesController(ArticleService articleService, CategoryService categoryService, CommentService commentService)
        {
            _articleService = articleService;
            _categoryService = categoryService;
            _commentService = commentService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index(string searchString = null)
        {
            IEnumerable<ArticleViewModel> articles;

            if (searchString == null)
            {
                articles = _articleService.GetAllArticles();
                return View(articles);
            }

            articles = _articleService.FindByKeywords(searchString);
            return View(articles);
        }

        [HttpGet]
        [Route("[controller]/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var articleViewModel = _articleService.GetArticleById(id);

            if (articleViewModel == null)
            {
                return NotFound();
            }

            return View(articleViewModel);
        }

        [HttpGet]
        [Route("[controller]/[action]")]
        public IActionResult Create()
        {
            ViewData["Category"] = new SelectList(_categoryService.GetAllCategories(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ArticleEditViewModel model, IFormFileCollection files = null)
        {
            if (ModelState.IsValid)
            {
                if (files != null)
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/");
                    foreach (var file in files)
                    {
                        var image = new FilePath
                        {
                            FileName = Guid.NewGuid() + Path.GetExtension(file.FileName),
                            FileType = FileType.Photo,
                        };
                        await using (Stream fileStream = new FileStream(path + image.FileName, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        model.FilePaths.Add(image);
                    }
                }
                await _articleService.SaveArticleAsync(model);
                return RedirectToAction(nameof(Index));
                    
            }
            ViewData["Category"] = new SelectList(_categoryService.GetAllCategories(), "Id", "Name");
            return View(model);
        }

        [Route("[controller]/{id}/[action]")]
        public IActionResult Edit(int id)
        {
            var article = _articleService.GetArticleToEditById(id);

            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,  ArticleEditViewModel model, List<IFormFile> files = null)
        {
            if (ModelState.IsValid)
            {
                    foreach (var file in files)
                    {
                        var image = new FilePath
                        {
                            FileName = System.IO.Path.GetFileName(file.FileName),
                            FileType = FileType.Photo,
                        };
                        model.FilePaths.Add(image);
                    }
                    await _articleService.UpdateArticleAsync(model);
                    return RedirectToAction(nameof(Index));
            }
            ViewData["Category"] = new SelectList(_categoryService.GetAllCategories(), "Id", "Name");
            return View(model);
        }

        [HttpGet]
        [Route("[controller]/{id}/[action]")]
        public IActionResult Delete(int id)
        {

            var article = _articleService.GetArticleById(id);
            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        [HttpPost]
        [ActionName("Delete")]
        [Route("[controller]/{id}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _articleService.DeleteArticleAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Comment(int articleId, string text)
        {
            await _commentService.LeaveComment(articleId, text);

            return NoContent();
        }

        [HttpGet]
        public IActionResult Pending()
        {
            return View();
        }

        [HttpGet]
        [Route("[controller]/Pending/{id}")]
        public IActionResult Review(int id)
        {
            var article = _articleService.GetArticleById(id);
            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }
    }
}
