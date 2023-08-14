public enum BombType
{
    None,
    Column,
    Row,
    Adjacent,
    Color
}

public enum MatchValue
{
    None,
    Yellow,
    Blue,
    Magenta,
    Indigo,
    Green,
    Teal,
    Red,
    Cyan,
    Orange,
    Purple,
    Wild
}

public enum TileType
{
    None,
    Normal,
    Obstacle,
    Breakable
}

public enum UIType
{
    Unknow = 0,
    Screen = 1,
    Popup = 2,
    Notify = 3,
    Overlap = 4,
}

public enum ListenType
{
    ANY = 0,
    UPDATECOUNTMOVE,
    UPDATESCORE
}