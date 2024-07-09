using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Text;

namespace FFXIVMultiLang.Sheets;

public partial class QuestText : ExcelRow
{
    public SeString Id { get; set; }
    public SeString Description { get; set; }

    public override void PopulateData(RowParser parser, GameData gameData, Language language)
    {
        base.PopulateData(parser, gameData, language);

        Id = parser.ReadColumn<SeString>(0);
        Description = parser.ReadColumn<SeString>(1);
    }
}
