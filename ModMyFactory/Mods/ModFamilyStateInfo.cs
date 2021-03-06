//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using Newtonsoft.Json;

namespace ModMyFactory.Mods
{
    /// <summary>
    /// Stores information about the state of a mod family.
    /// </summary>
    public sealed class ModFamilyStateInfo
    {
        /// <summary>
        /// Name of the family.
        /// </summary>
        [JsonProperty("name")]
        public string FamilyName { get; }

        /// <summary>
        /// Whether a mod in this family is enabled.
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; }

        /// <summary>
        /// The version of the enabled mod in the family. Undefined if no mod is enabled.
        /// </summary>
        [JsonProperty("version", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public AccurateVersion Version { get; }

        [JsonConstructor]
        private ModFamilyStateInfo(string name, bool enabled, AccurateVersion version)
            => (FamilyName, Enabled, Version) = (name, enabled, version);


        /// <summary>
        /// Creates state information for a mod family.
        /// </summary>
        public static ModFamilyStateInfo FromFamily(ModFamily family)
        {
            var name = family.FamilyName;
            bool enabled = false;
            AccurateVersion version = default;

            var enabledMod = family.EnabledMod;
            if (enabledMod != null)
            {
                enabled = true;
                var defaultMod = family.GetDefaultMod();
                if (enabledMod != defaultMod) version = enabledMod.Version; // Only store version if enabled mod is not default
            }

            return new ModFamilyStateInfo(name, enabled, version);
        }
    }
}
