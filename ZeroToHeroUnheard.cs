using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using System.Reflection;
using Path = System.IO.Path;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace ZeroToHeroUnheard
{

    [Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
    public class ZeroToHeroUnheard : IOnLoad
    {
        private readonly ISptLogger<ZeroToHeroUnheard> _logger;
        private readonly DatabaseServer _databaseServer;
        private readonly ModHelper _modHelper;
        private readonly string _modPath;

        public ZeroToHeroUnheard(ISptLogger<ZeroToHeroUnheard> logger, DatabaseServer databaseServer, ModHelper modHelper)
        {
            _logger = logger;
            _databaseServer = databaseServer;
            _modHelper = modHelper;
            _modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

            if (string.IsNullOrEmpty(_modPath))
            {
                _logger.Error("[JERO] ZeroToHeroUnheard - Não foi possível determinar o caminho do mod. O mod não fará alterações.");
            }
        }

        public Task OnLoad()
        {
            try
            {
                if (string.IsNullOrEmpty(_modPath))
                {
                    return Task.CompletedTask;
                }

                const string profileName = "Zero To Hero Unheard";
                
                var tables = _databaseServer.GetTables();
                var profiles = tables.Templates.Profiles;

                // Verifica se o perfil "Unheard" existe
                if (!profiles.TryGetValue("Unheard", out var unheardProfile))
                {
                    _logger.Error("[JERO] ZeroToHeroUnheard -  Perfil 'Unheard' não encontrado no banco de dados!");
                    return Task.CompletedTask;
                }

                // Carrega os dados de configuração usando ModHelper
                var bearInventory = _modHelper.GetJsonDataFromFile<BotBaseInventory>(_modPath, Path.Join("config", "bear_inventory.json"));
                var usecInventory = _modHelper.GetJsonDataFromFile<BotBaseInventory>(_modPath, Path.Join("config", "usec_inventory.json"));
                var traderStandingObj = _modHelper.GetJsonDataFromFile<ProfileTraderTemplate>(_modPath, Path.Join("config", "traders.json"));
                var description = _modHelper.GetJsonDataFromFile<string>(_modPath, Path.Join("config", "descLocale.json"));
                var skillIssueObj = _modHelper.GetJsonDataFromFile<Skills>(_modPath, Path.Join("config", "skill_issue.json"));

                // Usa reflexão para fazer uma cópia profunda do perfil sem serializar MongoId
                // Cria uma nova instância de ProfileSides e copia as propriedades
                var zthProfile = new ProfileSides();
                
                // Copia as propriedades usando reflexão para evitar problemas com MongoId
                var profileType = typeof(ProfileSides);
                
                foreach (var prop in profileType.GetProperties())
                {
                    try
                    {
                        var value = prop.GetValue(unheardProfile);
                        if (value != null && prop.CanWrite)
                        {
                            prop.SetValue(zthProfile, value);
                        }
                    }
                    catch
                    {
                        // Ignora propriedades que não podem ser copiadas
                    }
                }

                // Aplica as modificações diretamente nas propriedades do objeto
                // IMPORTANTE: Substitui o Trader DEPOIS de copiar para garantir que os valores do JSON sejam usados
                if (traderStandingObj != null)
                {
                    if (zthProfile.Bear != null)
                    {
                        zthProfile.Bear.Trader = traderStandingObj;
                    }
                    if (zthProfile.Usec != null)
                    {
                        zthProfile.Usec.Trader = traderStandingObj;
                    }
                }

                if (bearInventory != null && zthProfile.Bear?.Character != null)
                {
                    zthProfile.Bear.Character.Inventory = bearInventory;
                }

                if (usecInventory != null && zthProfile.Usec?.Character != null)
                {
                    zthProfile.Usec.Character.Inventory = usecInventory;
                }

                if (!string.IsNullOrEmpty(description))
                {
                    zthProfile.DescriptionLocaleKey = description;
                }

                if (skillIssueObj != null)
                {
                    if (zthProfile.Bear?.Character != null)
                    {
                        zthProfile.Bear.Character.Skills = skillIssueObj;
                    }
                    if (zthProfile.Usec?.Character != null)
                    {
                        zthProfile.Usec.Character.Skills = skillIssueObj;
                    }
                }

                // Adiciona o novo perfil ao banco de dados
                profiles[profileName] = zthProfile;
                _logger.Success("[JERO] ZeroToHeroUnheard - Carregado com sucesso!");
            }
            catch (Exception ex)
            {
                _logger.Error($"[JERO] ZeroToHeroUnheard - Erro ao carregar o mod: {ex.Message}");
                _logger.Error(ex.StackTrace ?? string.Empty);
            }

            return Task.CompletedTask;
        }
    }
}
