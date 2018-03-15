using System;
using System.Linq;
namespace Common
{
    public class Auth_
    {
        public const string auth = "auth";
        public static void setAuth(System.Collections.ArrayList account)
        {
            try { System.Web.HttpContext.Current.Session[auth] = account; }
            catch { return; }
        }
        public static System.Collections.ArrayList getUser()
        {
            try
            {
                var authss = (System.Collections.ArrayList)System.Web.HttpContext.Current.Session[auth];
                return (authss.Count > 0 ? authss : null);
            }
            catch { return null; }
        }
        //public static System.Collections.ArrayList user = (System.Collections.ArrayList)HttpContext.Current.Session[auth];
        public static Guid id()
        {
            try
            {
                return Guid.Parse(getUser()[0].ToString());
            }
            catch (Exception) { return Guid.Empty; }
        }
        public static string username()
        {
            try
            {
                return getUser()[1].ToString();
            }
            catch (Exception) { return null; }
        }
        public static string salt()
        {
            try
            {
                return getUser()[2].ToString();
            }
            catch (Exception) { return null; }
        }
        public static string full_name()
        {
            try
            {
                return getUser()[3].ToString();
            }
            catch (Exception) { return null; }
        }
        public static string mobile()
        {
            try
            {
                return getUser()[4].ToString();
            }
            catch (Exception) { return null; }
        }
        public static string email()
        {
            try
            {
                return getUser()[5].ToString();
            }
            catch (Exception) { return null; }
        }
        public static string address()
        {
            try
            {
                return getUser()[6].ToString();
            }
            catch (Exception) { return null; }
        }
        public static string roles()
        {
            try
            {
                return getUser()[7].ToString();
            }
            catch (Exception) { return null; }
        }
        public static string created_by()
        {
            try
            {
                return getUser()[8].ToString();
            }
            catch (Exception) { return null; }
        }
        public static string created_at()
        {
            try
            {
                return getUser()[9].ToString();
            }
            catch (Exception) { return null; }
        }
        public static string updated_by()
        {
            try
            {
                return getUser()[10].ToString();
            }
            catch (Exception) { return null; }
        }
        public static string updated_at()
        {
            try
            {
                return getUser()[11].ToString();
            }
            catch (Exception) { return null; }
        }
        public static string last_login()
        {
            try
            {
                return getUser()[12].ToString();
            }
            catch (Exception) { return null; }
        }
        public static string flag()
        {
            try
            {
                return getUser()[13].ToString();
            }
            catch (Exception) { return null; }
        }
        public static Guid staff_id()
        {
            try
            {
                return Guid.Parse(getUser()[14].ToString());
            }
            catch (Exception) { return Guid.Empty; }
        }
        public static bool isAuth()
        {
            if (System.Web.HttpContext.Current.Session[auth] != null)
                return true;
            return false;
        }
        public static bool inRoles(string[] r)
        {
            if (r.Contains(roles()))
                return true;
            return false;
        }
        public static void logout()
        {
            try { System.Web.HttpContext.Current.Session[auth] = null; }
            catch { return; }
        }
    }
    public class AuthStatic
    {
        public static System.Guid id = Guid.Parse("f4191f70-2c4a-442e-a62d-b4b6833b33f4");
        public const string username = "tuanmjnh";
        public const string password = "aa2de065c899d53d7031b0975c56062f";//"Tuanmjnh@tm"; //"fc44d0279133a2f46178134ce9bf2167";//tuanmjnh@123
        public const string salt = "1c114c58-69d9-41e6-bd3e-363906e04e50";
        public const string full_name = "SuperAdmin";
        public const string mobile = "0123456789";
        public const string email = "SuperAdmin@SuperAdmin.com";
        public const string address = "SuperAdmin";
        public const string roles = Authentication.Roles.superadmin;
        public const string created_by = "f4191f70-2c4a-442e-a62d-b4b6833b33f4";
        //public const string created_at = DateTime.Now;
        public const string updated_by = "f4191f70-2c4a-442e-a62d-b4b6833b33f4";
        //public const string updated_at = "SuperAdmin";
        //public const string last_login = "SuperAdmin";
        public const int flag = 1;
        public static bool isAuthStatic(string username, string password)
        {
            if (AuthStatic.username == username && AuthStatic.password == TM.Encrypt.CryptoMD5TM(password + AuthStatic.salt))
                return true;
            return false;
        }
    }
    public class roles
    {
        public const string superadmin = "187eb627-0a7b-44a8-83c4-ceb4829709a3";
        public const string admin = "ee82e7f1-592c-4f5c-95fa-7cad30b14a2d";
        public const string managerBill = "114EE7A8-29AF-4CC5-ACC8-1C12456F0755";
        public const string mod = "238391cd-990d-4f3b-8d0c-0300416f9263";
        public const string director = "121ab8e5-1ad2-4b78-8ff2-4d953c9b71a8";
        public const string manager = "3a32dc87-eb43-45f0-9bea-fb9030afeaf0";
        public const string leader = "d0443498-09c4-4267-a7c9-2a20dde8e925";
        public const string staff = "dc67601d-ad74-4813-8293-8d4a07db1d31";
    }
}
