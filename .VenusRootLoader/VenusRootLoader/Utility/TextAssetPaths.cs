namespace VenusRootLoader.Utility;

/// <summary>
/// Contains important resources paths from the game that other services can refer to.
/// These are case-insensitive because Unity treats them as such.
/// </summary>
public static class TextAssetPaths
{
    internal const string RootAnimatorControllerdPathPrefix = "AnimationControllers/";
    internal const string RootAudioPathPrefix = "Audio/";
    internal const string RootDataPathPrefix = "Data/";
    internal const string RootFontdPathPrefix = "Fonts/";
    internal const string RootMaterialsPathPrefix = "Materials/";
    internal const string RootPrefabsPathPrefix = "Prefabs/";
    internal const string RootShadersPathPrefix = "Shaders/";
    internal const string RootSpritesPathPrefix = "Sprites/";

    internal const string AudioMusicDirectory = "Music";
    internal const string AudioSoundsDirectory = "Sounds";
    internal const string AudioSoundsDialogueDirectory = $"{AudioSoundsDirectory}/Dialogue";

    internal const string DataLocalizedDialoguesDirectoryPrefix = "Dialogues";
    internal const string DataSlashDialogues = $"{RootDataPathPrefix}Dialogues";
    internal const string DataMapEntitiesDirectory = "EntityData";
    internal const string DataSlashEntityData = $"{RootDataPathPrefix}EntityData";
    internal const string DataFishingItemsPath = "FishingItems";
    internal const string DataFishingFishesDirectory = "Fishing/Fish";
    internal const string DataMedalsPath = "BadgeData";
    internal const string DataMedalsOrderingPath = "BadgeOrder";
    internal const string DataQuestsPath = "BoardData";
    internal const string DataSpyCardsPath = "CardData";
    internal const string DataSpyCardsOrderingPath = "CardOrder";
    internal const string DataRecipesLibraryEntriesInputItemsPath = "CookLibrary";
    internal const string DataRecipesLibraryEntriesResultItemsPath = "CookOrder";
    internal const string DataDiscoveriesOrderingPath = "DiscoveryOrder";
    internal const string DataEnemiesPath = "EnemyData";
    internal const string DataAnimIdsPath = "EntityValues";
    internal const string DataItemsPath = "ItemData";
    internal const string DataLanguageHelpTextsPath = "LanguageHelp";
    internal const string DataBattleLeafPositionsInTransitionPath = "LeafPos";
    internal const string DataLetterPromptPathPrefix = "LetterPrompt";
    internal const string DataRankBonusesPath = "LevelData";
    internal const string DataMusicLoopPointsPath = "LoopPoints";
    internal const string DataQuestsRequirementsPath = "QuestChecks";
    internal const string DataRecipesPath = "RecipeData";
    internal const string DataSkillsPath = "SkillData";
    internal const string DataRecordsOrderingPath = "SynopsisOrder";
    internal const string DataBestiaryEntriesOrderingPath = "TattleList";
    internal const string DataTermacadePrizesPath = "Termacade";
    internal const string DataTestRoomMapDialoguesPath = "TestRoom";
    internal const string DataCaveOfTrialsBattlesPath = "Trials";

    internal const string DataDialoguesLocalizedMapsDirectory = "Maps";
    internal const string DataLocalizedActionCommandHelpTextsPathSuffix = "ActionCommands";
    internal const string DataLocalizedAreaDescriptionsPathSuffix = "AreaDesc";
    internal const string DataLocalizedAreaNamesPathSuffix = "AreaNames";
    internal const string DataLocalizedMedalPathSuffix = "BadgeName";
    internal const string DataLocalizedQuestsPathSuffix = "BoardQuests";
    internal const string DataLocalizedSpyCardsTextsPathSuffix = "CardDialogue";
    internal const string DataLocalizedSpyCardsPathSuffix = "CardText";
    internal const string DataLocalizedCommonDialoguesPathSuffix = "CommonDialogue";
    internal const string DataLocalizedCreditsPathSuffix = "Credits";
    internal const string DataLocalizedDiscoveriesPathSuffix = "Discoveries";
    internal const string DataLocalizedBestiaryEntriesPathSuffix = "EnemyTattle";
    internal const string DataLocalizedFishingTextsPathSuffix = "Fishing";
    internal const string DataLocalizedCrystalBerryFortuneTellerHintsPathSuffix = "FortuneTeller0";
    internal const string DataLocalizedLoreBookFortuneTellerHintsPathSuffix = "FortuneTeller1";
    internal const string DataLocalizedMedalFortuneTellerHintsPathSuffix = "FortuneTeller2";
    internal const string DataLocalizedItemsPathSuffix = "Items";
    internal const string DataLocalizedLoreBooksPathSuffix = "LoreText";
    internal const string DataLocalizedMenuTextsPathSuffix = "MenuText";
    internal const string DataLocalizedMusicNamesPathSuffix = "MusicList";
    internal const string DataLocalizedSkillsPathSuffix = "Skills";
    internal const string DataLocalizedRecordsPathSuffix = "Synopsis";

    internal const string PrefabsBattleMapsDirectory = "BattleMaps";
    internal const string PrefabsDisguisesDirectory = "Disguises";
    internal const string PrefabsMapsDirectory = "Maps";
    internal const string PrefabsObjectsDirectory = "Objects";
    internal const string PrefabsParticlesDirectory = "Particles";

    internal const string SpritesEntitiesDirectory = "Entities";
    internal const string SpritesGuiDirectory = "Gui";
    internal const string SpritesEnemyPortraitsPath = "Items/EnemyPortraits";
    internal const string SpritesItems0Path = "Items/Items0";
    internal const string SpritesItems1Path = "Items/Items1";

    internal const string SpritesGui9BoxDirectory = "Gui/9Box";
    internal const string SpritesBattleMessagePathPrefix = "Gui/BattleMessage";
    internal const string SpritesLeafInBattleTransitionPath = "Gui/BattleLeaves";
    internal const string SpritesEmoticonsPath = "Gui/Emoticons";
    internal const string SpritesGui1Path = "Gui/Gui";
    internal const string SpritesGui2Path = "Gui/Gui2";
    internal const string SpritesTextBoxPath = "Gui/Textbox";
    internal const string SpritesLocalizesGameTitlePathPrefix = "Gui/Title";

    internal const string SpritesLocalizesBattleMessageAssetPrefix = "Battlem";
    internal const string SpritesLocalizesBattleMessageRankUpAssetPrefix = "Rank";
}