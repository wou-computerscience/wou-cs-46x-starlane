﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using iCollections.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using iCollections.Data;

namespace iCollections.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        private ICollectionsDbContext collectionsDb;

        public HomeController(ILogger<HomeController> logger, UserManager<IdentityUser> userManager, ICollectionsDbContext db)
        {
            _logger = logger;
            _userManager = userManager;
            collectionsDb = db;
        }

        public async Task<IActionResult> Index()
        {
            bool isAuthenticated = User.Identity.IsAuthenticated;
            if (isAuthenticated)
            {
                //return RedirectToAction("Index", "DashboardController");
                return RedirectToAction("Index", "Dashboard");
            }
            /*            // Information straight from the Controller (does not need to do to the database)
                        bool isAdmin = User.IsInRole("Admin");
                        bool isAuthenticated = User.Identity.IsAuthenticated;
                        string name = User.Identity.Name;
                        string authType = User.Identity.AuthenticationType;

                        // Information from Identity through the user manager
                        string id = _userManager.GetUserId(User);         // reportedly does not need to hit db
                        IdentityUser user = await _userManager.GetUserAsync(User);  // does go to the db
                        string email = user?.Email ?? "no email";
                        string phone = user?.PhoneNumber ?? "no phone number";
                        ViewBag.Message = $"User {name} is authenticated? {isAuthenticated} using type {authType} and is an" +
                                          $" Admin? {isAdmin}. ID from Identity {id}, email is {email}, and phone is {phone}";*/
            return View();
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public int GetICollectionUserID(string id) {
            // get user with id
            var user = collectionsDb.IcollectionUsers.First(i => i.AspnetIdentityId == id);
            int numericUserId = user.Id;
            // get numerical id and return
            return numericUserId;
        }

        public IcollectionUser SelectFriend(IcollectionUser user1, IcollectionUser user2, int myId) {
            if (user1.Id == myId) return user2;
            return user1;
        }

        public bool KeyInFriendship(IcollectionUser user1, IcollectionUser user2, int key) {
            return key == user1.Id || key == user2.Id;
        }

        // Dashboard opens here - shows a feed of recent events
        public IActionResult Feed()
        {
            // string nastyStringId = _userManager.GetUserId(User);
            // int userId = GetICollectionUserID(nastyStringId);
            int userId = 2;     // my hardcoded userId
            var accountCollections = collectionsDb.Collections
                    .Where(collection => collection.UserId == userId)
                    .OrderBy(collection => collection.DateMade)
                    .ToList();

            // get all events and order each list by date using .OrderBy(c => c.Date)

            // display who our friends became friends with

            //get list of my friends
            var myFriends = collectionsDb.FriendsWiths
                .Where(friendship => friendship.User1.Id == userId || friendship.User2.Id == userId)
                .Select(friendship => SelectFriend(friendship.User1, friendship.User2, userId))
                .ToList();

            // have three lists by now

            var myFriendsFriends = collectionsDb.FriendsWiths
                .Where(friendship => myFriends.Any(friend => KeyInFriendship(friendship.User1, friendship.User2, friend.Id) && !KeyInFriendship(friendship.User1, friendship.User2, userId)))
                .ToList();
            
            // get who we follow follows


            var whoIFollow = collectionsDb.Follows
                .Where(f => f.FollowerNavigation.Id == userId)
                .Select(f => f.FollowedNavigation)
                .ToList();

            var topFollow = collectionsDb.Follows
                .Where(f => whoIFollow.Any(myFollowee => myFollowee.Id == f.FollowerNavigation.Id))
                .ToList();

            // put all lists inside a Model

            

            // pass it to View

            // in View - loop through 3 lists at same time "popping" the most recent event from
            // its list and render its info

            return View();
        }
    }
}
