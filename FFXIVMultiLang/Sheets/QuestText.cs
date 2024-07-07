using Dalamud.Game.Text.SeStringHandling;
using Lumina;
using Lumina.Data;
using Lumina.Excel;
namespace FFXIVMultiLang.Sheets;

public partial class QuestText : ExcelRow
{
    public string Id { get; set; }
    public string Description { get; set; }

    public override void PopulateData(RowParser parser, GameData gameData, Language language)
    {
        base.PopulateData(parser, gameData, language);

        Id = parser.ReadColumn<string>(0);
        Description = parser.ReadColumn<string>(1);
    }
}
