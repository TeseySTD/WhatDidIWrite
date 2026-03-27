using Wdiw.Infrastructure.Models;

namespace Wdiw.Infrastructure.Abstractions;

public interface IConfigRepository
{
   UserSettings GetSettings();
   void SaveSettings(UserSettings settings);
}