using System;

namespace FD.Blazor.Examples
{
    public static class Extensions
    {
        /// <summary>
        /// Converts the seconds to an min seconds display string.
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <returns>
        /// A string in the format minutes'seconds''.
        /// </returns>
        public static string DurationToString(this TimeSpan timeSpan)
        {
            var s = TimeSpan.FromSeconds(timeSpan.TotalSeconds);

            return string.Format("{0}'{1}''", (int)s.TotalMinutes, s.Seconds);
        }
    }
}
