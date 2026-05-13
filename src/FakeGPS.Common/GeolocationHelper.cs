namespace FakeGPS.Common
{
    using System;
    using System.Device.Location;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Threading;

    /// <summary>
    /// Static class to help with Geolocation operations.
    /// </summary>
    public static class GeolocationHelper
    {
        // http://stackoverflow.com/questions/3518504/regular-expression-for-matching-latitude-longitude-coordinates
        private static Regex regex = new Regex(@"^([-+]?\d{1,2}([.]\d+)?),\s*([-+]?\d{1,3}([.]\d+)?)$");

        /// <summary>
        /// Checks to see if the location string is valid.
        /// </summary>
        /// <param name="latLong">The location string.</param>
        /// <returns>
        /// A value indicating whether the location string was valid.
        /// </returns>
        public static bool IsValid(string latLong)
        {
            // protect IsMatch from null
            if (string.IsNullOrWhiteSpace(latLong))
            {
                return false;
            }

            // will return false if invalid
            return regex.IsMatch(latLong);
        }

        /// <summary>
        /// Convert a location string to a <see cref="LatLong"/>.
        /// </summary>
        /// <param name="latLong">The location string.</param>
        /// <returns>
        /// The <see cref="LatLong"/> instance.
        /// </returns>
        public static LatLong ToLatLong(string latLong)
        {
            if (!IsValid(latLong))
            {
                throw new ArgumentException("The provided LatLong string is not in a valid format.");
            }

            try
            {
                // ok we've got a well formated latLong string
                var splits = latLong.Split(',');

                return new LatLong()
                {
                    Latitude = Convert.ToDouble(splits[0], CultureInfo.InvariantCulture),
                    Longitude = Convert.ToDouble(splits[1], CultureInfo.InvariantCulture)
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get the current location from the Windows location API.
        /// </summary>
        /// <returns>
        /// The <see cref="LatLong"/> instance.
        /// </returns>
        public static LatLong Get()
        {
            using (var watcher = new GeoCoordinateWatcher())
            {
                // Start the watcher and wait up to 1 second for it to initialize
                watcher.TryStart(false, TimeSpan.FromMilliseconds(1000));

                GeoCoordinate coord = watcher.Position.Location;

                // If the coordinate is initially unknown, wait for the API to find a fix
                if (coord.IsUnknown)
                {
                    using (var waitHandle = new ManualResetEvent(false))
                    {
                        EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>> handler = null;
                        handler = (s, e) =>
                        {
                            if (!e.Position.Location.IsUnknown)
                            {
                                coord = e.Position.Location;
                                waitHandle.Set(); // Found a location, wake up the thread
                            }
                        };

                        watcher.PositionChanged += handler;

                        // Wait up to 10 seconds for a valid position fix from Windows
                        waitHandle.WaitOne(10000);

                        watcher.PositionChanged -= handler;
                    }
                }

                // If it's still unknown, throw an exception instead of returning NaN
                if (coord.IsUnknown)
                {
                    throw new Exception("Unable to determine location. Please ensure 'Location Services' and 'Let desktop apps access your location' are both ON in Windows Privacy Settings.");
                }

                return new LatLong()
                {
                    Latitude = coord.Latitude,
                    Longitude = coord.Longitude
                };
            }
        }
    }
}