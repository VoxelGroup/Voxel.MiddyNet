﻿using System.Text.Json.Serialization;

namespace Voxel.MiddyNet.HttpV2JsonBodyParserSample
{
    public class Person
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("surname")]
        public string Surname { get; set; }
        [JsonPropertyName("age")]
        public int Age { get; set; }

    }
}