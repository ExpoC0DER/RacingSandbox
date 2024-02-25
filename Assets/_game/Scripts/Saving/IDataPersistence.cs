namespace _game.Scripts.Saving
{
    public interface IDataPersistence
    {
        void LoadLevel(LevelData data);
        void SaveLevel(LevelData data);
    }
}
