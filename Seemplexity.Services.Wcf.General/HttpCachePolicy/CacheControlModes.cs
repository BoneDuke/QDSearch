namespace Seemplexity.Services.Wcf.HttpCachePolicy
{
    public enum CacheControlModes
    {
        /// <summary>
        /// Does not add a Cache-Control or Expires header to the response.
        /// The numeric value is 0.
        /// </summary>
        NoControl = 0,
        /// <summary>
        /// Adds a Cache-Control: no-cache header to the response.
        /// The numeric value is 1.
        /// </summary>
        DisableCache = 1,
        /// <summary>
        /// Adds a Cache-Control: max-age header to the response based on the value specified in the CacheControlMaxAge attribute.
        /// The numeric value is 2.
        /// </summary>
        UseMaxAge = 2,
        /// <summary>
        /// Adds an Expires: date header to the response based on the date specified in the httpExpires attribute.
        /// The numeric value is 3.
        /// </summary>
        UseExpires = 3
    }
}