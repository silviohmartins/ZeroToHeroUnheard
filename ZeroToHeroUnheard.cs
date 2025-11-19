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

                // Carrega os arquivos JSON de configuração usando ModHelper
                var bearInventoryData = _modHelper.GetJsonDataFromFile<object>(_modPath, Path.Join("config", "bear_inventory.json"));
                var usecInventoryData = _modHelper.GetJsonDataFromFile<object>(_modPath, Path.Join("config", "usec_inventory.json"));
                var traderStanding = _modHelper.GetJsonDataFromFile<object>(_modPath, Path.Join("config", "traders.json"));
                var description = _modHelper.GetJsonDataFromFile<string>(_modPath, Path.Join("config", "descLocale.json"));
                var skillIssue = _modHelper.GetJsonDataFromFile<object>(_modPath, Path.Join("config", "skill_issue.json"));

                // Usa reflexão para fazer uma cópia profunda do perfil sem serializar MongoId
                // Cria uma nova instância de ProfileSides e copia as propriedades
                var zthProfile = new ProfileSides();
                
                // Copia as propriedades usando reflexão para evitar problemas com MongoId
                var profileType = typeof(ProfileSides);
                var unheardProfileType = unheardProfile.GetType();
                
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
                // Usa o ModHelper para deserializar diretamente para os tipos corretos
                // Isso evita problemas com MongoId porque o ModHelper usa as configurações corretas do SPT
                if (bearInventoryData != null && zthProfile.Bear?.Character != null)
                {
                    // Tenta usar o ModHelper para deserializar diretamente
                    var bearInventory = _modHelper.GetJsonDataFromFile<BotBaseInventory>(_modPath, Path.Join("config", "bear_inventory.json"));
                    if (bearInventory != null)
                    {
                        zthProfile.Bear.Character.Inventory = bearInventory;
                    }
                }

                if (usecInventoryData != null && zthProfile.Usec?.Character != null)
                {
                    var usecInventory = _modHelper.GetJsonDataFromFile<BotBaseInventory>(_modPath, Path.Join("config", "usec_inventory.json"));
                    if (usecInventory != null)
                    {
                        zthProfile.Usec.Character.Inventory = usecInventory;
                    }
                }

                if (traderStanding != null)
                {
                    var traderStandingObj = _modHelper.GetJsonDataFromFile<ProfileTraderTemplate>(_modPath, Path.Join("config", "traders.json"));
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
                }

                if (!string.IsNullOrEmpty(description))
                {
                    zthProfile.DescriptionLocaleKey = description;
                }

                if (skillIssue != null)
                {
                    var skillIssueObj = _modHelper.GetJsonDataFromFile<Skills>(_modPath, Path.Join("config", "skill_issue.json"));
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
