/// <summary>
///    This class is used to store all the assets used in our projects
///    A simple way to get the asset path by using reflection.
///    e.g. Console.WriteLine(Assets.Moviestarplanet.Diamond)
///    </summary>


using System.Collections.Concurrent;
using System.Reflection;

namespace Dolo;

public static partial class Assets
{
     /// <summary>
     /// The base URL for the assets.
     /// </summary>
     private const string _baseUrl = "https://raw.githubusercontent.com/cydolo/assets/main/";

     /// <summary>
     /// We use a cache to store the asset path.
     /// </summary>
     private static readonly ConcurrentDictionary<string, string> _assetCache = new();


     /// <summary>
     ///  moviestarplanet assets
     /// </summary>
     [Asset]
     public static class Moviestarplanet
     {
          [Asset("diamond.png")] public static string Diamond => GetAsset();
     }

     /// <summary>
     /// moviestarplanet-swf assets
     /// </summary>
     [Asset]
     public static class MoviestarplanetSwf
     {
          [Asset("school_yard.swf")] public static string SchoolYard => GetAsset();
          [Asset("the_lobby.swf")] public static string TheLobby => GetAsset();
          [Asset("video_top.swf")] public static string VideoTop => GetAsset();
     }


     /// <summary>
     ///  moviestarplanet-components assets
     /// </summary>
     [Asset]
     public static class MoviestarplanetComponents
     {
          [Asset]
          public static class Login
          {
               [Asset("citybackground.svg")] public static string CityBackground => GetAsset();
               [Asset("createuserlight.svg")] public static string CreateUserLight => GetAsset();
          }

          [Asset]
          public static class Preloader
          {
               [Asset("background.png")] public static string Background => GetAsset();
          }
     }


     /// <summary>
     /// We use simple reflection to get the asset path.
     /// By using this way we only need to define an asset file name in the attribute.
     /// </summary>
     private static string GetAsset([CallerMemberName] string memberName = "")
     {
          return _assetCache.GetOrAdd(memberName, _ =>
          {
               var property = typeof(AssetsNew)
                 .GetNestedTypes(BindingFlags.Public | BindingFlags.Static)
                 .SelectMany(t => t.GetNestedTypes(BindingFlags.Public | BindingFlags.Static).Concat([t]))
                 .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Static))
                 .FirstOrDefault(p => p.Name == memberName)
                 ?? throw new InvalidOperationException($"Property {memberName} not found");

               var assetPath = property.DeclaringType?
                 .GetCustomAttributes<Asset>()
                 .Any() is true
                     ? string.Join("/",
                         GetTypeHierarchy(property.DeclaringType)
                             .TakeWhile(t => t != typeof(AssetsNew))
                             .Where(t => t.GetCustomAttribute<Asset>() is not null)
                             .Select(t => Regex.Replace(t.Name, "([a-z])([A-Z])", "$1-$2").ToLower())
                             .Reverse())
                     : string.Empty;

               var assetFile = property.GetCustomAttribute<Asset>()?.File;
               return $"{_baseUrl}{assetPath}/{assetFile}";
          });
     }


     /// <summary>
     /// Get the type hierarchy of a type.
     /// </summary>
     private static IEnumerable<Type> GetTypeHierarchy(Type type)
     {
          while (type != null)
          {
               yield return type;
               type = type.DeclaringType;
          }
     }


     /// <summary>
     ///  Our Asset Attribute
     /// </summary>
     [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
     public class Asset : Attribute
     {
          public Asset()
          { }

          public Asset(string file)
          {
               File = file;
          }

          public string? File { get; set; }
     }
}
