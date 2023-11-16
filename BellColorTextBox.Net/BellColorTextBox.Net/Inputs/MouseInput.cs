using System.Numerics;

namespace Bell.Inputs;

public enum MouseAction
{
    None,
    
    Click,
    DoubleClick,
    Dragging,
}

public enum MouseCursor
{
    Arrow,
    Beam,
    Hand
}

public struct MouseInput
{
    public MouseAction LeftAction;
    public MouseAction MiddleAction;

    public Vector2 Position;
}