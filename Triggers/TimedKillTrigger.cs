using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DzhakeHelper.Triggers
{
    [CustomEntity("DzhakeHelper/TimedKillTrigger")]

    public class TimedKillTrigger : Trigger
    {

        public float LiveTime;

        public Color color;

        public bool OnlyOnGround;


        public TimedKillTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            base.Depth = -5; // 
            LiveTime = data.Float("liveTime");
            color = data.HexColorWithAlpha("color");
            OnlyOnGround = data.Bool("onlyOnGround");
        }

        public override void OnStay(Player player)
        {
            if ((OnlyOnGround && player.OnGround()) || !OnlyOnGround)
            {
                if (!DzhakeHelperModule.Session.TimedKillTriggerTimeChanged)
                {
                    DzhakeHelperModule.Session.TimedKillTriggerTime += Engine.DeltaTime;
                    DzhakeHelperModule.Session.TimedKillTriggerTimeChanged = true;
                }
                if (DzhakeHelperModule.Session.TimedKillTriggerTime > LiveTime)
                {
                    player.Die(Position);
                }
                if (DzhakeHelperModule.Session.TimedKillTriggerMaxTime < LiveTime)
                {
                    DzhakeHelperModule.Session.TimedKillTriggerMaxTime = LiveTime;
                    DzhakeHelperModule.Session.TimedKillTriggerColor = DzhakeHelperModule.Session.TimedKillTriggerColor.Mix(color, (DzhakeHelperModule.Session.TimedKillTriggerTime / DzhakeHelperModule.Session.TimedKillTriggerMaxTime) / 2f);
                }
            }
        }
    }
}
