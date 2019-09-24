namespace Fanex.Bot.Helpers
{
    using System;

    public static class DataHelper
    {
        public static T Parse<T>(object data)
                where T : class
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var model = data as T;

            if (model == null)
            {
                throw new InvalidOperationException($"Cannot parse {typeof(T)} model");
            }

            return model;
        }

        public static T TryParse<T>(object data)
                where T : class
        {
            if (data == null)
            {
                return null;
            }

            return Parse<T>(data);
        }
    }
}