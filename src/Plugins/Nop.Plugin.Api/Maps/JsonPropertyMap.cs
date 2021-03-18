﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core.Caching;
using Nop.Core.Infrastructure;
using Nop.Plugin.Api.Attributes;
using static Nop.Plugin.Api.Infrastructure.Constants;

namespace Nop.Plugin.Api.Maps
{
    public class JsonPropertyMapper : IJsonPropertyMapper
    {
        private IStaticCacheManager _cacheManager;

        private IStaticCacheManager StaticCacheManager
        {
            get
            {
                if (_cacheManager == null)
                {
                    _cacheManager = EngineContext.Current.Resolve<IStaticCacheManager>();
                }

                return _cacheManager;
            }
        }
        public Dictionary<string, Tuple<string, Type>> GetMap(Type type)
        {
            //if (!StaticCacheManager.IsSet(Configurations.JsonTypeMapsPattern))
            //{
            //    StaticCacheManager.Set(Configurations.JsonTypeMapsPattern, new Dictionary<string, Dictionary<string, Tuple<string, Type>>>(), int.MaxValue);
            //}

            var typeMaps =  StaticCacheManager.GetAsync(Configurations.JsonTypeMapsPattern, () => new Dictionary<string, Dictionary<string, Tuple<string, Type>>>()).Result;

            if (!typeMaps.ContainsKey(type.Name))
            {
                Build(type);
            }

            return typeMaps[type.Name];
        }

        private void Build(Type type)
        {
            var typeMaps = StaticCacheManager.GetAsync(Configurations.JsonTypeMapsPattern, () => new Dictionary<string, Dictionary<string, Tuple<string, Type>>>()).Result;

            var mapForCurrentType = new Dictionary<string, Tuple<string, Type>>();

            var typeProps = type.GetProperties();

            foreach (var property in typeProps)
            {
                var jsonAttribute = property.GetCustomAttribute(typeof(JsonPropertyAttribute)) as JsonPropertyAttribute;
                var doNotMapAttribute = property.GetCustomAttribute(typeof(DoNotMapAttribute)) as DoNotMapAttribute;

                // If it has json attribute set and is not marked as doNotMap
                if (jsonAttribute != null && doNotMapAttribute == null)
                {
                    if (!mapForCurrentType.ContainsKey(jsonAttribute.PropertyName))
                    {
                        var value = new Tuple<string, Type>(property.Name, property.PropertyType);
                        mapForCurrentType.Add(jsonAttribute.PropertyName, value);
                    }
                }
            }

            if (!typeMaps.ContainsKey(type.Name))
            {
                typeMaps.Add(type.Name, mapForCurrentType);
            }
        }
        public async Task<Dictionary<string, Tuple<string, Type>>> GetMapAsync(Type type)
        {
            //if (!StaticCacheManager.IsSet(Configurations.JsonTypeMapsPattern))
            //{
            //    StaticCacheManager.Set(Configurations.JsonTypeMapsPattern, new Dictionary<string, Dictionary<string, Tuple<string, Type>>>(), int.MaxValue);
            //}

            var typeMaps = await StaticCacheManager.GetAsync(Configurations.JsonTypeMapsPattern, () => new Dictionary<string, Dictionary<string, Tuple<string, Type>>>());

            if (!typeMaps.ContainsKey(type.Name))
            {
                await BuildAsync(type);
            }

            return typeMaps[type.Name];
        }

        private async Task BuildAsync(Type type)
        {
            var typeMaps = await StaticCacheManager.GetAsync(Configurations.JsonTypeMapsPattern, () => new Dictionary<string, Dictionary<string, Tuple<string, Type>>>());

            var mapForCurrentType = new Dictionary<string, Tuple<string, Type>>();

            var typeProps = type.GetProperties();

            foreach (var property in typeProps)
            {
                var jsonAttribute = property.GetCustomAttribute(typeof(JsonPropertyAttribute)) as JsonPropertyAttribute;
                var doNotMapAttribute = property.GetCustomAttribute(typeof(DoNotMapAttribute)) as DoNotMapAttribute;

                // If it has json attribute set and is not marked as doNotMap
                if (jsonAttribute != null && doNotMapAttribute == null)
                {
                    if (!mapForCurrentType.ContainsKey(jsonAttribute.PropertyName))
                    {
                        var value = new Tuple<string, Type>(property.Name, property.PropertyType);
                        mapForCurrentType.Add(jsonAttribute.PropertyName, value);
                    }
                }
            }

            if (!typeMaps.ContainsKey(type.Name))
            {
                typeMaps.Add(type.Name, mapForCurrentType);
            }
        }
    }
}