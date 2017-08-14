using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bot1350.DBServises
{
    public class FDH_BotService
    {
        private FDH_botEntities dbContext;

        public FDH_BotService()
        {
            this.dbContext = new FDH_botEntities();
        }

        public void AddUserCookie(string name, string cookie)
        {
            var u = GetUserCookie(name);
            if (u != null)
            {
                u.UserCookie = cookie;
            }
            else
            {
                User user = new User();
                user.UserName = name;
                user.UserCookie = cookie;
                this.dbContext.Users.Add(user);
            }
            this.dbContext.SaveChanges();
        }

        public User GetUserCookie(string name)
        {
            var user = this.dbContext.Users.Where(p => p.UserName == name).FirstOrDefault();
            if (user != null)
            {
                return user;
            }
            else
            {
                return null;
            }
        }

        public void AddDataToWishList(string name, string wish, string cookie)
        {
            try
            {
                User u = GetUserCookie(name);
                if (u == null)
                {
                    AddUserCookie(name, cookie);
                    AddDataToWishList(name, wish, cookie);
                }
                else
                {
                    WishList wishItem = new WishList();
                    wishItem.UserId = u.UserId;
                    wishItem.Wish = wish;
                    this.dbContext.WishLists.Add(wishItem);
                    this.dbContext.SaveChanges();
                }
            }
            catch (Exception e)
            {

            }
        }

        public string GetUsersWish()
        {
            try
            {
                var wishs = this.dbContext.WishLists.ToList();
                var resultStr = "";
                foreach (WishList wish in wishs)
                {
                    resultStr += $"{wish.User.UserName}: {wish.Wish}\n\n________________________\n\n";
                }
                return resultStr;
            }
            catch (Exception e)
            {
                return "";
            }
        }

        public void AddItemsToShopList(string items, string name, string cookie)
        {
            try
            {
                User u = GetUserCookie(name);
                if (u == null)
                {
                    AddUserCookie(name, cookie);
                    AddItemsToShopList(name, items, cookie);
                }
                else
                {
                    ShopingList shopingItems = new ShopingList();
                    shopingItems.DateTime = DateTime.UtcNow;
                    shopingItems.ItemsList = items;
                    shopingItems.UserId = u.UserId;
                    this.dbContext.ShopingLists.Add(shopingItems);
                    this.dbContext.SaveChanges();
                }
            }
            catch (Exception e)
            {

            }
        }

        public string GetShopingItems(DateTime startDate)
        {
            try
            {
                var shopingItems = this.dbContext.ShopingLists.Where(p => p.DateTime > startDate).ToList();
                var resultStr = "";
                foreach (ShopingList item in shopingItems)
                {
                    resultStr += $"{item.User.UserName}: {item.ItemsList}\n\n{item.DateTime}\n\n________________________\n\n";
                }
                return resultStr;
            }
            catch (Exception e)
            {
                return "";
            }
        }

        public void AddDataToLog()
        {
            try
            {
                Log log = new Log();
                log.ErrorMessage = "1111";
                this.dbContext.Logs.Add(log);
                this.dbContext.SaveChanges();
            }
            catch (Exception e)
            {

            }
        }
    }
}