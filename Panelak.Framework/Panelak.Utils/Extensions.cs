﻿namespace Panelak.Utils
{
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// Service extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Adds the proxy services
        /// </summary>
        /// <param name="serviceCollection">Service collection</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddProxySupport(this IServiceCollection services)
            => services.AddSingleton<IProxyService, ProxyService>();

        /// <summary>
        /// Adds the proxy service
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="proxyUri">URL of proxy</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddProxySupport(this IServiceCollection services, string proxyUri)
            => services.Configure<ProxyOptions>(options =>
            {
                options.ProxyUrl = proxyUri;
            }).AddSingleton<IProxyService, ProxyService>();

        /// <summary>
        /// Adds the proxy service
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="config">Proxy configuration section</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddProxySupport(this IServiceCollection services, IConfigurationSection config)
            => services.Configure<ProxyOptions>(config)
                       .AddSingleton<IProxyService, ProxyService>();

        /// <summary>
        /// Adds the recaptcha services
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Recaptcha configuration section</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddRecaptchaService(this IServiceCollection services, IConfigurationSection configuration) 
            => services.Configure<RecaptchaOptions>(configuration)
                       .AddSingleton<IRecaptchaTokenValidationService, RecaptchaValidationService>()
                       .AddSingleton<RecaptchaFilter>();

        /// <summary>
        /// Adds the recaptcha services
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="active">Whether recaptcha is active</param>
        /// <param name="secretKey">Secret key</param>
        /// <param name="siteKey">Site key</param>
        /// <param name="throwExceptions">True if service throw exception on HTTP request fail for google API. False = return false on token validation instead.</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddRecaptchaService(this IServiceCollection services, bool active, string secretKey, string siteKey, bool throwExceptions = true) 
            => services.Configure<RecaptchaOptions>(options =>
                {
                    options.Active = active;
                    options.SecretKey = secretKey;
                    options.SiteKey = siteKey;
                    options.ThrowExceptions = throwExceptions;
                })
                .AddSingleton<IRecaptchaTokenValidationService, RecaptchaValidationService>()
                .AddSingleton<RecaptchaFilter>();

        /// <summary>
        /// Adds datetime provider
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddDateTimeProvider(this IServiceCollection services) 
            => services.AddSingleton<IDateTimeProvider, DefaultDateTimeProvider>();

        /// <summary>
        /// Adds the AspNetCore page tree provider
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="controllersAssembly">Assembly to read Route attributes in controllers from</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddPageTree(this IServiceCollection services, Assembly controllersAssembly)
        {
            var provider = new PageTreeProvider(controllersAssembly);
            services.AddSingleton(provider);
            services.AddSingleton<IPageTreeProvider>(provider);
            services.AddScoped<ICurrentPageProvider, CurrentPageProvider>();
            return services;
        }


        /// <summary>
        /// Adds the AspNetCore page tree provider
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="controllersAssembly">Assembly to read Route attributes in controllers from</param>
        /// <param name="baseControllerType">Only controllers with this base type are going to be analyzed</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddPageTree(this IServiceCollection services, Assembly controllersAssembly, Type baseControllerType)
        {
            var provider = new PageTreeProvider(controllersAssembly, baseControllerType);
            services.AddSingleton(provider);
            services.AddSingleton<IPageTreeProvider>(provider);
            services.AddScoped<ICurrentPageProvider, CurrentPageProvider>();
            return services;
        }

        /// <summary>
        /// Serializes the object to XML using <see cref="XmlSerializer"/>.
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="value">Instance of object</param>
        /// <returns>Serialized XML</returns>
        public static string ToXml<T>(this T value)
        {
            if (value == null)
                return String.Empty;
            
            var xmlserializer = new XmlSerializer(typeof(T));
            var stringWriter = new StringWriter();
            using (var writer = XmlWriter.Create(stringWriter))
            {
                xmlserializer.Serialize(writer, value);
                return stringWriter.ToString();
            }
        }

        /// <summary>
        /// Gets the service of the defined type parameter from the service provider
        /// </summary>
        /// <typeparam name="T">Type parameter</typeparam>
        /// <param name="serviceProvider">Service provider</param>
        /// <returns>Service</returns>
        public static T GetService<T>(this IServiceProvider serviceProvider) where T : class
            => serviceProvider.GetService(typeof(T)) as T;

        /// <summary>
        /// Gets the service of the defined type parameter from the service provider found in the view context.
        /// </summary>
        /// <typeparam name="T">Type parameter</typeparam>
        /// <param name="viewContext">View context</param>
        /// <returns>Service</returns>
        public static T GetService<T>(this ViewContext viewContext) where T : class
            => viewContext.HttpContext.RequestServices.GetService<T>();


        /// <summary>
        /// Tries to parse a string to a nullable enum value. If parsing fails,
        /// returns the null value instead.
        /// </summary>
        /// <typeparam name="T">Type of enum</typeparam>
        /// <param name="value">String to parse</param>
        /// <param name="result">Nullable enum</param>
        /// <returns>True if parsing succeeded, false otherwise.</returns>
        public static bool TryParseNullableEnum<T>(this string value, out T? result) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new Exception("This method is only for Enums");

            if (Enum.TryParse(value, out T tempResult))
            {
                result = tempResult;
                return true;
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Attempts to parse the nullable enum. If the parsing fails, returns null instead.
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        /// <param name="value">String to parse</param>
        /// <returns>Enum if parsing succeeds, null otherwise.</returns>
        public static T? ParseNullableEnum<T>(this string value) where T : struct, IConvertible
        {
            if (Enum.TryParse(value, out T tempResult))
                return tempResult;

            return null;
        }
    }
}
