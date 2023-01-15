using BenMakesGames.MonoGame.Palettes;
using BenMakesGames.PlayPlayMini;
using BenMakesGames.PlayPlayMini.Services;
using Client.Models;
using Client.Services;
using Microsoft.Xna.Framework;
using Shared;

namespace Client.GameStates;

// sealed classes execute faster than non-sealed, so always seal your game states!
public sealed class Playing: GameState
{
    private GraphicsManager Graphics { get; }
    private KeyboardManager Keyboard { get; }
    private GameStateManager GSM { get; }
    private ServerClient Server { get; }

    private Guid PlayerId { get; } = Guid.NewGuid();

    private List<Player> Players { get; } = new List<Player>();

    public Playing(GraphicsManager graphics, GameStateManager gsm, KeyboardManager keyboard, ServerClient server)
    {
        Graphics = graphics;
        GSM = gsm;
        Keyboard = keyboard;
        Server = server;

        Server.Send<StopMovingClientCommand>(PlayerId, new());
    }

    // overriding lifecycle methods is optional; feel free to delete any overrides you're not using.
    // note: you do NOT need to call the `base.` for lifecycle methods. so save some CPU cycles,
    // and don't call them :P

    public override void ActiveInput(GameTime gameTime)
    {
    }

    public override void ActiveUpdate(GameTime gameTime)
    {
        // TODO: update game objects based on user input, AI logic, etc
    }

    public override void AlwaysUpdate(GameTime gameTime)
    {
    }

    public override void ActiveDraw(GameTime gameTime)
    {
        Graphics.Clear(DawnBringers16.Black);

        foreach (var p in Players)
        {
            Graphics.DrawFilledRectangle(p.PixelX - 5, p.PixelY - 5, 11, 11, DawnBringers16.Blue);
        }
    }

    public override void AlwaysDraw(GameTime gameTime)
    {
        // TODO: draw game scene (refer to PlayPlayMini documentation for more info)
    }

    public override void Enter()
    {
    }

    public override void Leave()
    {
    }
}