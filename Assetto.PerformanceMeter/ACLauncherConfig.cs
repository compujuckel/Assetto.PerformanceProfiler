namespace Assetto.PerformanceMeter;

public class ACLauncherConfig
{
    public static string GetRaceCfg(string track, string layout, string car, string skin)
    {
        return $"""
                [BENCHMARK]
                ACTIVE=0

                [RACE]
                AI_LEVEL=100
                CARS=1
                CONFIG_TRACK={layout}
                DRIFT_MODE=0
                FIXED_SETUP=0
                JUMP_START_PENALTY=0
                MODEL={car}
                MODEL_CONFIG=
                PENALTIES=0
                RACE_LAPS=2
                SKIN={skin}
                TRACK={track}

                [DYNAMIC_TRACK]
                LAP_GAIN=1
                RANDOMNESS=0
                SESSION_START=100
                SESSION_TRANSFER=100

                [GHOST_CAR]
                ENABLED=0
                FILE=
                LOAD=0
                PLAYING=0
                RECORDING=0
                SECONDS_ADVANTAGE=0

                [GROOVE]
                VIRTUAL_LAPS=10
                MAX_LAPS=1
                STARTING_LAPS=1

                [HEADER]
                VERSION=2

                [LAP_INVALIDATOR]
                ALLOWED_TYRES_OUT=-1

                [LIGHTING]
                CLOUD_SPEED=0
                SUN_ANGLE=-16.00
                TIME_MULT=0

                [OPTIONS]
                USE_MPH=0

                [REPLAY]
                ACTIVE=0

                [RESTART]
                ACTIVE=0

                [TEMPERATURE]
                AMBIENT=26
                ROAD=37

                [WEATHER]
                NAME=3_clear

                [WIND]
                DIRECTION_DEG=0
                SPEED_KMH_MAX=0
                SPEED_KMH_MIN=0

                [SESSION_0]
                NAME=Practice
                TYPE=1
                DURATION_MINUTES=0
                SPAWN_SET=PIT
                """;
    }
}
