using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ThreadsApp.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static IHtmlContent RelativeDate(this IHtmlHelper htmlHelper, DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan <= TimeSpan.FromSeconds(60))
                return new HtmlString(string.Format("{0} seconds ago", timeSpan.Seconds));

            if (timeSpan <= TimeSpan.FromMinutes(60))
                return new HtmlString(timeSpan.Minutes > 1 ? String.Format("about {0} minutes ago", timeSpan.Minutes) : "about a minute ago");

            if (timeSpan <= TimeSpan.FromHours(24))
                return new HtmlString(timeSpan.Hours > 1 ? String.Format("about {0} hours ago", timeSpan.Hours) : "about an hour ago");

            if (timeSpan <= TimeSpan.FromDays(30))
                return new HtmlString(timeSpan.Days > 1 ? String.Format("about {0} days ago", timeSpan.Days) : "yesterday");

            if (timeSpan <= TimeSpan.FromDays(365))
                return new HtmlString(timeSpan.Days > 30 ? String.Format("about {0} months ago", timeSpan.Days / 30) : "about a month ago");

            return new HtmlString(timeSpan.Days > 365 ? String.Format("about {0} years ago", timeSpan.Days / 365) : "about a year ago");
        }
    }
}
