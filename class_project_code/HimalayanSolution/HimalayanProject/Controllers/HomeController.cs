﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using HimalayanProject.Models;
using Microsoft.EntityFrameworkCore;

namespace HimalayanProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private HimalayanContext db;

        public HomeController(ILogger<HomeController> logger, HimalayanContext context)
        {
            _logger = logger;
            db = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Search(string search_term)
        {
            if (search_term != null)
            {

                //Search expeditions by peak name or trekking agency name. 
                IEnumerable<Expedition> result = db.Expeditions.Include(p => p.Peak).Where(p => p.Peak.Name.Contains(search_term)).OrderByDescending(O => O.Year).AsEnumerable();
                return View("Index", result);
            }
            else 
            {
                return View("Index");
            }
            
        }
    }
}