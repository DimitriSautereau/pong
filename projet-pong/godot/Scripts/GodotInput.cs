using Core;
using Godot;

namespace GodotApp;


// Implémentation de IInput pour Godot

public partial class GodotInput : IInput
{
    public float GetPlayer1Direction()
    {
        if (Input.IsActionPressed("p1_up")) return -1;
        if (Input.IsActionPressed("p1_down")) return 1;
        return 0;
    }

    public float GetPlayer2Direction()
    {
        if (Input.IsActionPressed("p2_up")) return -1;
        if (Input.IsActionPressed("p2_down")) return 1;
        return 0;
    }

    public bool ShouldRestart()
    {
        return Input.IsActionJustPressed("restart");
    }
}