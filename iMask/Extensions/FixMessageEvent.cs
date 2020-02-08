using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FixMessageEvent
{
    public static class MessageEventExtension
    {
        public static EventMessage Fix(this EventMessage eventMessage)
        {
            switch (eventMessage)
            {
                case MediaEventMessage media:
                    {
                        if (media.Type == EventMessageType.Image)
                            return new ImageEventMessage(
                                media.Type,
                                media.Id,
                                media.ContentProvider,
                                media.Duration);
                        if (media.Type == EventMessageType.Video)
                            return new VideoEventMessage(
                                media.Type,
                                media.Id,
                                media.ContentProvider,
                                media.Duration);
                        if (media.Type == EventMessageType.Audio)
                            return new AudioEventMessage(
                                media.Type,
                                media.Id,
                                media.ContentProvider,
                                media.Duration);
                    }
                    break;
            }
            return eventMessage;
        }
    }

    public class ImageEventMessage : MediaEventMessage
    {
        public ImageEventMessage(
            EventMessageType type,
            string id,
            ContentProvider contentProvider = null,
            int? duration = null
            ) : base(type, id, contentProvider, duration)
        {
        }
    }

    public class VideoEventMessage : MediaEventMessage
    {
        public VideoEventMessage(
            EventMessageType type,
            string id,
            ContentProvider contentProvider = null,
            int? duration = null
            ) : base(type, id, contentProvider, duration)
        {
        }
    }

    public class AudioEventMessage : MediaEventMessage
    {
        public AudioEventMessage(
            EventMessageType type,
            string id,
            ContentProvider contentProvider = null,
            int? duration = null
            ) : base(type, id, contentProvider, duration)
        {
        }
    }
}