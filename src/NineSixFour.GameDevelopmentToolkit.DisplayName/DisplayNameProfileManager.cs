using System;
using System.Collections.Generic;

namespace NineSixFour.GameDevelopmentToolkit.DisplayName
{
    /// <summary>
    /// Manages named display name validation profiles.
    /// </summary>
    public class DisplayNameProfileManager
    {
        private readonly Dictionary<string, DisplayNameOptions> _profiles = 
            new Dictionary<string, DisplayNameOptions>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Registers a profile with the given name and options.
        /// </summary>
        public void Register(string profileName, DisplayNameOptions options)
        {
            if (string.IsNullOrWhiteSpace(profileName))
                throw new ArgumentException("Profile name cannot be null or whitespace.", nameof(profileName));
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            _profiles[profileName] = options;
        }

        /// <summary>
        /// Tries to get options for a profile.
        /// </summary>
        public bool TryGet(string profileName, out DisplayNameOptions? options)
        {
            if (string.IsNullOrWhiteSpace(profileName))
            {
                options = null;
                return false;
            }
            return _profiles.TryGetValue(profileName, out options);
        }

        /// <summary>
        /// Gets options for a profile, or returns a new default if not found.
        /// </summary>
        public DisplayNameOptions GetOrDefault(string profileName)
        {
            if (TryGet(profileName, out var options) && options != null)
                return options;
            return new DisplayNameOptions();
        }

        /// <summary>
        /// Gets a validator for a profile.
        /// </summary>
        public DisplayNameValidator GetValidator(string profileName)
        {
            var options = GetOrDefault(profileName);
            return new DisplayNameValidator(options);
        }

        /// <summary>
        /// Removes a profile.
        /// </summary>
        public bool Remove(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName))
                return false;
            return _profiles.Remove(profileName);
        }

        /// <summary>
        /// Clears all profiles.
        /// </summary>
        public void Clear()
        {
            _profiles.Clear();
        }
    }
}
