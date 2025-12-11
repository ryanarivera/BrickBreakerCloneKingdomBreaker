using System.Collections.Generic;
using UnityEngine;

// Global registry for all active PlayerBall objects
public static class PlayerBallRegistry
{
    public static List<PlayerBall> Balls = new List<PlayerBall>();

    public static void Register(PlayerBall ball)
    {
        if (!Balls.Contains(ball))
            Balls.Add(ball);
    }

    public static void Unregister(PlayerBall ball)
    {
        if (Balls.Contains(ball))
            Balls.Remove(ball);
    }
}
