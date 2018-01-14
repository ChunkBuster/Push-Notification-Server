﻿using System;
using System.Collections.Generic;

namespace PushNotificationServer.Notifications {
    [Serializable]
    public class ClientInfo {
        public string Version;

        /// <summary>
        ///     Get the version info as an array
        /// </summary>
        /// <returns>Version Info array</returns>
        public int[] GetVersionInfo() {
            return Array.ConvertAll(Version.Split('.'), int.Parse);
        }

        /// <inheritdoc />
        public override int GetHashCode() {
            var version = GetVersionInfo();
            var hash = version[0] * 17;
            hash *= version[1] * 17;
            hash *= version[2] * 17;
            return hash;
        }
    }

    [Serializable]
    public class NotificationInfo {
        public List<Notification> Notifications;

        public NotificationInfo() {
            Notifications = new List<Notification>();
        }

        /// <summary>
        ///     Add a new notification
        /// </summary>
        /// <param name="message">message to add</param>
        /// <param name="type">type of notification</param>
        /// <param name="id">ID of notification</param>
        public void AddNotification(string message, int type, int id) {
            Notifications.Add(new Notification(message, type, id));
        }

        /// <summary>
        ///     Add a new notification
        /// </summary>
        /// <param name="n">The notification to add</param>
        public void AddNotification(Notification n) {
            Notifications.Add(n);
        }

        [Serializable]
        public class Notification : IComparable {
            public int Id;
            public string Message;
            public int Type;

            public Notification(string message, int type, int id) {
                Message = message;
                Type = type;
                Id = id;
            }

            /// <inheritdoc />
            public int CompareTo(object obj) {
                if (obj == null) return 1;
                if (obj is Notification other)
                    return other.Type - Type;
                throw new ArgumentException("Object is not a Notification");
            }
        }
    }
}