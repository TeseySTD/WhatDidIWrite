using Wdiw.Infrastructure.Models;

namespace Wdiw.Infrastructure.Abstractions;

public interface IConfigRepository
{
   AppSettings GetSettings();
   void SaveSettings(AppSettings settings);
}