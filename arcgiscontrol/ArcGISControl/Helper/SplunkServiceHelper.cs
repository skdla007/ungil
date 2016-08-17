using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ArcGISControl.Helper
{
    using ArcGISControls.CommonData.Models;
    using Innotive.SplunkManager.SplunkManager;
    using Innotive.SplunkManager.SplunkManager.Data;

    public abstract class SplunkServiceHandler
    {
        /// <summary>
        /// Splunk Service가 활성화 중일때 OjectDataID를 넣는다.
        /// string : ID
        /// </summary>
        private List<string> splunkServiceStatus = new List<string>(); 

        /// <summary>
        /// 스플렁크 서비스 실행
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="splunkBasicInformation"></param>
        /// <param name="doPlay">true : view ,  false : onetime </param>
        /// <param name="schedulingInterval"></param>
        public void StartSplunkService(string objectId, SplunkBasicInformationData splunkBasicInformation, bool doPlay, int? schedulingInterval = null)
        {
            if (this.IsOnStartSplunkService(objectId)) return;

            if (!SplunkBasicInformationData.IsUsableServiceInfo(splunkBasicInformation)) return;

            var savedSearchArgs = new SplunkSavedSearchArgs()
            {
                Name = splunkBasicInformation.Name,
                SearchIndex = objectId,
                TimeLinePlayWay = doPlay
                                    ? SplunkSavedSearchArgs.PlayWayMode.Play
                                    : SplunkSavedSearchArgs.PlayWayMode.Seek
            };

            if(schedulingInterval != null)
            {
                savedSearchArgs.SchedulingInterval = schedulingInterval.Value;
            }

            this.StartSplunkService(objectId, splunkBasicInformation, savedSearchArgs);
        }

        /// <summary>
        /// Playback 상태에서 스플렁크 서비스 실행
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="splunkBasicInformation"></param>
        /// <param name="timeSpan"></param>
        /// <param name="doPlay"></param>
        /// <param name="operationPlaybackArgs"></param>
        /// <param name="schedulingInterval"></param>
        public void StartSplunkService(string objectId, SplunkBasicInformationData splunkBasicInformation,
            TimeSpan timeSpan, bool doPlay, OperationPlaybackArgs operationPlaybackArgs, int? schedulingInterval = null)
        {
            if (this.IsOnStartSplunkService(objectId)) return;

            if (!SplunkBasicInformationData.IsUsableServiceInfo(splunkBasicInformation)) return;

            var savedSearchArgs = new SplunkSavedSearchArgs()
            {
                Name = splunkBasicInformation.Name,
                SearchIndex = objectId,
                TimeLineTimeSpan = timeSpan,
                TimeLinePlayWay = doPlay
                        ? SplunkSavedSearchArgs.PlayWayMode.Play
                        : SplunkSavedSearchArgs.PlayWayMode.Seek
            };

            ApplyOperationPlaybackArgs(savedSearchArgs, operationPlaybackArgs);

            if (schedulingInterval != null)
            {
                savedSearchArgs.SchedulingInterval = schedulingInterval.Value;
            }

            this.StartSplunkService(objectId, splunkBasicInformation, savedSearchArgs);
        }

        /// <summary>
        /// 스플렁크 서비스 시작
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="splunkBasicInformation"></param>
        /// <param name="savedSearchArgs"></param>
        protected virtual void StartSplunkService(string objectId, SplunkBasicInformationData splunkBasicInformation, SplunkSavedSearchArgs savedSearchArgs)
        {
            for (int i = 0; i < splunkBasicInformation.SplArgumentKeys.Count; i++)
            {
                var key = splunkBasicInformation.SplArgumentKeys.ElementAt(i);
                var var = splunkBasicInformation.SplArgumentValues.ElementAt(i);

                savedSearchArgs.SavedSearchArgs.Add(key, var);
            }

            var args = new SplunkServiceFactoryArgs
            {
                Host = splunkBasicInformation.IP,
                Port = splunkBasicInformation.Port,
                App = splunkBasicInformation.App,
                UserName = splunkBasicInformation.UserId,
                UserPwd = splunkBasicInformation.Password
            };

            SplunkServiceFactory.Instance.GetSplunkService(args).BeginExecuteSavedSearch
                    (
                        (resultSet) 
                            => Application.Current.Dispatcher.Invoke(
                            new Action(() =>
                                    {
                                        try
                                        {
                                            this.SetSplunkCallbackData(objectId, splunkBasicInformation, resultSet);
                                        }
                                        catch (Exception exception)
                                        {
                                            InnowatchDebug.Logger.WriteInfoLog(exception.ToString());
                                        }
                                    })), savedSearchArgs
                    );

            if(!splunkServiceStatus.Contains(objectId))
            {
                splunkServiceStatus.Add(objectId);
            }
        }

        /// <summary>
        /// 스플렁크 서비스 정지
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="splunkBasicInformation"></param>
        public void StopSplunkService(string objectId, SplunkBasicInformationData splunkBasicInformation)
        {
            if (!this.IsOnStartSplunkService(objectId)) return;

            if (!SplunkBasicInformationData.IsUsableServiceInfo(splunkBasicInformation)) return;

            var args = new SplunkServiceFactoryArgs
            {
                Host = splunkBasicInformation.IP,
                Port = splunkBasicInformation.Port,
                App = splunkBasicInformation.App,
                UserName = splunkBasicInformation.UserId,
                UserPwd = splunkBasicInformation.Password
            };

            SplunkServiceFactory.Instance.GetSplunkService(args).BeginExecuteAbortSearchThread(objectId);

            if (splunkServiceStatus.Contains(objectId))
            {
                splunkServiceStatus.Remove(objectId);
            }
        }

        /// <summary>
        /// Splunk Service가 Start 되었는지 확인한다.
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        protected bool IsOnStartSplunkService(string objectId)
        {
            return splunkServiceStatus.Contains(objectId);
        }

        internal static void ApplyOperationPlaybackArgs(SplunkSavedSearchArgs destSavedSearchArgs, OperationPlaybackArgs operationPlaybackArgs)
        {
            if (operationPlaybackArgs == null)
                return;

            destSavedSearchArgs.OperationPlaybackArgs.PreMinutes = operationPlaybackArgs.PreMinutes;
            destSavedSearchArgs.OperationPlaybackArgs.PostMinutes = operationPlaybackArgs.PostMinutes;
        }

        /// <summary>
        /// 모든 스플렁크 서비스 정지
        /// </summary>
        abstract public void StopAllSplunkServices();

        /// <summary>
        /// 스플렁크 서비스에서의 Callback 함수
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="splunkBasicInformation"></param>
        /// <param name="splunkResultSet"></param>
        protected abstract void SetSplunkCallbackData(string objectId, SplunkBasicInformationData splunkBasicInformation, SplunkResultSet splunkResultSet);
    }
}
