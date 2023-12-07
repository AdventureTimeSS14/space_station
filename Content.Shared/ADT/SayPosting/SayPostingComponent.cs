using System.Numerics;

namespace Content.Shared.ADT
{

    [RegisterComponent, Access(typeof(SayPostingSystem))]
    public sealed partial class SayPostingComponent : Component
    {
        [DataField("timeDelaySayPosting", required: true)]

        public Vector2 TimeDelaySayPosting { get; private set; }


        [DataField("durationSayPosting", required: true)]
        public Vector2 DurationSayPosting { get; private set; }

        public float NextOfTime;
    }


}