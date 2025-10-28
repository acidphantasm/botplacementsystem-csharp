using System.Runtime.CompilerServices;
using _botplacementsystem.Web.Shared;

namespace _botplacementsystem.Web.Core
{
    internal class Utils
    {
        public static List<string> callerList = new List<string>();

        public static IEnumerable<string> StringObjectIDValidation(string value)
        {
            if (!string.IsNullOrEmpty(value) && (value.Length != 24 || !IsHex(value)))
            {
                yield return "Invalid MongoID";
            }
        }
        public static IEnumerable<string> StringLengthValidation(string value)
        {
            if (!string.IsNullOrEmpty(value) && (value.Length >= 19))
            {
                yield return "Invalid, Name too long";
            }
        }

        public static bool IsHex(IEnumerable<char> chars)
        {
            bool isHex;
            foreach (var c in chars)
            {
                isHex = ((c >= '0' && c <= '9') ||
                         (c >= 'a' && c <= 'f') ||
                         (c >= 'A' && c <= 'F'));

                if (!isHex)
                {
                    Console.WriteLine(isHex);
                    return false;
                }
            }
            return true;
        }

        public static bool IsHexAndValidLength(string value)
        {
            if (value.Length == 24 && IsHex(value)) return true;
            else return false;
        }

        public static bool IsStringAndValidLength(string value)
        {
            if (value.Length <= 19) return true;
            else return false;
        }

        public static void UpdateViewBool(bool holder, bool actual)
        {
            if (holder != actual)
            {
                MainLayout.EnableUnsavedChangesButton();
            }
        }
        
        public static void UpdateViewBool(bool? holder, bool? actual)
        {
            if (holder != actual)
            {
                MainLayout.EnableUnsavedChangesButton();
            }
        }
        public static void UpdateView(bool holder, bool originalConfigValue, [CallerMemberName] string caller = "")
        {
            switch (MainLayout.pendingChanges.Contains(caller))
            {
                case true:
                    if (holder != originalConfigValue) return;
                    if (holder == originalConfigValue)
                    {
                        MainLayout.pendingChanges.Remove(caller);
                    }
                    break;
                case false:
                    if (holder != originalConfigValue)
                    {
                        MainLayout.pendingChanges.Add(caller);
                    }
                    if (holder == originalConfigValue)
                    {
                        MainLayout.pendingChanges.Remove(caller);
                    }
                    break;
            }

            MainLayout.TriggerUIRefresh();
        }
        public static void UpdateView(bool? holder, bool? originalConfigValue, [CallerMemberName] string caller = "")
        {
            switch (MainLayout.pendingChanges.Contains(caller))
            {
                case true:
                    if (holder != originalConfigValue) return;
                    if (holder == originalConfigValue)
                    {
                        MainLayout.pendingChanges.Remove(caller);
                    }
                    break;
                case false:
                    if (holder != originalConfigValue)
                    {
                        MainLayout.pendingChanges.Add(caller);
                    }
                    if (holder == originalConfigValue)
                    {
                        MainLayout.pendingChanges.Remove(caller);
                    }
                    break;
            }

            MainLayout.TriggerUIRefresh();
        }
        public static void UpdateView(int holder, int originalConfigValue, [CallerMemberName] string caller = "")
        {
            switch (MainLayout.pendingChanges.Contains(caller))
            {
                case true:
                    if (holder != originalConfigValue) return;
                    if (holder == originalConfigValue)
                    {
                        MainLayout.pendingChanges.Remove(caller);
                    }
                    break;
                case false:
                    if (holder != originalConfigValue)
                    {
                        MainLayout.pendingChanges.Add(caller);
                    }
                    if (holder == originalConfigValue)
                    {
                        MainLayout.pendingChanges.Remove(caller);
                    }
                    break;
            }

            MainLayout.TriggerUIRefresh();
        }
        
        public static void UpdateView(double holder, double originalConfigValue, [CallerMemberName] string caller = "")
        {
            switch (MainLayout.pendingChanges.Contains(caller))
            {
                case true:
                    if (holder != originalConfigValue) return;
                    if (holder == originalConfigValue)
                    {
                        MainLayout.pendingChanges.Remove(caller);
                    }
                    break;
                case false:
                    if (holder != originalConfigValue)
                    {
                        MainLayout.pendingChanges.Add(caller);
                    }
                    if (holder == originalConfigValue)
                    {
                        MainLayout.pendingChanges.Remove(caller);
                    }
                    break;
            }

            MainLayout.TriggerUIRefresh();
        }
        public static void UpdateView(List<string> holder, List<string> originalConfigValue, [CallerMemberName] string caller = "")
        {
            switch (MainLayout.pendingChanges.Contains(caller))
            {
                case true:
                    if (!holder.SequenceEqual(originalConfigValue)) return;
                    if (holder.SequenceEqual(originalConfigValue))
                    {
                        MainLayout.pendingChanges.Remove(caller);
                    }
                    break;
                case false:
                    if (!holder.SequenceEqual(originalConfigValue))
                    {
                        MainLayout.pendingChanges.Add(caller);
                    }
                    break;
            }

            MainLayout.TriggerUIRefresh();
        }
        public static void UpdateView(List<int> holder, List<int> originalConfigValue, [CallerMemberName] string caller = "")
        {
            switch (MainLayout.pendingChanges.Contains(caller))
            {
                case true:
                    if (!holder.SequenceEqual(originalConfigValue)) return;
                    if (holder.SequenceEqual(originalConfigValue))
                    {
                        MainLayout.pendingChanges.Remove(caller);
                    }
                    break;
                case false:
                    if (!holder.SequenceEqual(originalConfigValue))
                    {
                        MainLayout.pendingChanges.Add(caller);
                    }
                    break;
            }

            MainLayout.TriggerUIRefresh();
        }
        public static void UpdateView(string holder, string originalConfigValue, [CallerMemberName] string caller = "")
        {
            switch (MainLayout.pendingChanges.Contains(caller))
            {
                case true:
                    if (holder != originalConfigValue) return;
                    if (holder == originalConfigValue)
                    {
                        MainLayout.pendingChanges.Remove(caller);
                    }
                    break;
                case false:
                    if (holder != originalConfigValue)
                    {
                        MainLayout.pendingChanges.Add(caller);
                    }
                    if (holder == originalConfigValue)
                    {
                        MainLayout.pendingChanges.Remove(caller);
                    }
                    break;
            }

            MainLayout.TriggerUIRefresh();
        }
    }
}
