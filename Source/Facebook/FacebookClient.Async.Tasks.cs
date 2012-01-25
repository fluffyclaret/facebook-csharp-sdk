﻿// --------------------------------
// <copyright file="FacebookClient.Async.Tasks.cs" company="Thuzi LLC (www.thuzi.com)">
//     Microsoft Public License (Ms-PL)
// </copyright>
// <author>Nathan Totten (ntotten.com), Jim Zimmerman (jimzimmerman.com) and Prabir Shrestha (prabir.me)</author>
// <license>Released under the terms of the Microsoft Public License (Ms-PL)</license>
// <website>https://github.com/facebook-csharp-sdk/facbook-csharp-sdk</website>
// ---------------------------------

namespace Facebook
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public partial class FacebookClient
    {
        /// <summary>
        /// Makes an asynchronous request to the Facebook server.
        /// </summary>
        /// <param name="httpMethod">Http method. (GET/POST/DELETE)</param>
        /// <param name="path">The resource path or the resource url.</param>
        /// <param name="parameters">The parameters</param>
        /// <param name="resultType">The type of deserialize object into.</param>
        /// <param name="userState">The user state.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
#if ASYNC_AWAIT
        /// <param name="uploadProgress">The upload progress</param>
#endif
        /// <returns>The task of json result with headers.</returns>
        protected virtual Task<object> ApiTaskAsync(string httpMethod, string path, object parameters, Type resultType, object userState, CancellationToken cancellationToken
#if ASYNC_AWAIT
, IProgress<FacebookUploadProgressChangedEventArgs> uploadProgress
#endif
)
        {
            var tcs = new TaskCompletionSource<object>(userState);
            if (cancellationToken.IsCancellationRequested)
            {
                tcs.TrySetCanceled();
                return tcs.Task;
            }

            httpMethod = httpMethod.ToUpperInvariant();

            HttpWebRequestWrapper httpWebRequest = null;
            EventHandler<HttpWebRequestCreatedEventArgs> httpWebRequestCreatedHandler = null;
            httpWebRequestCreatedHandler += (o, e) =>
                                                {
                                                    if (e.UserState != tcs)
                                                        return;
                                                    httpWebRequest = e.HttpWebRequest;
                                                };

            var ctr = cancellationToken.Register(() =>
                                                     {
                                                         try
                                                         {
                                                             if (httpWebRequest != null) httpWebRequest.Abort();
                                                         }
                                                         catch
                                                         {
                                                         }
                                                     });

#if ASYNC_AWAIT
                EventHandler<FacebookUploadProgressChangedEventArgs> uploadProgressHandler = null;
                if (uploadProgress != null)
                {
                    uploadProgressHandler = (sender, e) =>
                                            {
                                                if (e.UserState != tcs)
                                                    return;
                                                uploadProgress.Report(new FacebookUploadProgressChangedEventArgs(e.BytesReceived, e.TotalBytesToReceive, e.BytesSent, e.TotalBytesToSend, e.ProgressPercentage, userState));
                                            };

                    UploadProgressChanged += uploadProgressHandler;
                }
#endif

            EventHandler<FacebookApiEventArgs> handler = null;
            handler = (sender, e) =>
            {
                TransferCompletionToTask(tcs, e, e.GetResultData, () =>
                {
                    if (ctr != null) ctr.Dispose();
                    RemoveTaskAsyncHandlers(httpMethod, handler);
                    HttpWebRequestWrapperCreated -= httpWebRequestCreatedHandler;
#if ASYNC_AWAIT
                    if (uploadProgressHandler != null) UploadProgressChanged -= uploadProgressHandler;
#endif
                });
            };

            if (httpMethod == "GET")
                GetCompleted += handler;
            else if (httpMethod == "POST")
                PostCompleted += handler;
            else if (httpMethod == "DELETE")
                DeleteCompleted += handler;
            else
                throw new ArgumentOutOfRangeException("httpMethod");

            HttpWebRequestWrapperCreated += httpWebRequestCreatedHandler;

            try
            {
                ApiAsync(httpMethod, path, parameters, resultType, tcs);
            }
            catch
            {
                RemoveTaskAsyncHandlers(httpMethod, handler);
                HttpWebRequestWrapperCreated -= httpWebRequestCreatedHandler;
#if ASYNC_AWAIT
                    if (uploadProgressHandler != null) UploadProgressChanged -= uploadProgressHandler;
#endif
                throw;
            }

            return tcs.Task;
        }

#if ASYNC_AWAIT

        /// <summary>
        /// Makes an asynchronous request to the Facebook server.
        /// </summary>
        /// <param name="httpMethod">Http method. (GET/POST/DELETE)</param>
        /// <param name="path">The resource path or the resource url.</param>
        /// <param name="parameters">The parameters</param>
        /// <param name="resultType">The type of deserialize object into.</param>
        /// <param name="userState">The user state.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task of json result with headers.</returns>
        public virtual Task<object> ApiTaskAsync(string httpMethod, string path, object parameters, Type resultType, object userState, CancellationToken cancellationToken)
        {
            return ApiTaskAsync(httpMethod, path, parameters, resultType, userState, cancellationToken, null);
        }

#endif

        private static void TransferCompletionToTask<T>(System.Threading.Tasks.TaskCompletionSource<T> tcs, System.ComponentModel.AsyncCompletedEventArgs e, Func<T> getResult, Action unregisterHandler)
        {
            if (e.UserState != tcs)
                return;

            try
            {
                unregisterHandler();
            }
            finally
            {
                if (e.Cancelled) tcs.TrySetCanceled();
                else if (e.Error != null) tcs.TrySetException(e.Error);
                else tcs.TrySetResult(getResult());
            }
        }

        private void RemoveTaskAsyncHandlers(string httpMethod, EventHandler<FacebookApiEventArgs> handler)
        {
            if (httpMethod == "GET")
                GetCompleted -= handler;
            else if (httpMethod == "POST")
                PostCompleted -= handler;
            else if (httpMethod == "DELETE")
                DeleteCompleted -= handler;
        }

        public virtual Task<object> GetTaskAsync(string path)
        {
            return GetTaskAsync(path, null, CancellationToken.None);
        }

        public virtual Task<object> GetTaskAsync(object parameters)
        {
            return GetTaskAsync(null, parameters, CancellationToken.None);
        }

        public virtual Task<object> GetTaskAsync(string path, object parameters)
        {
            return GetTaskAsync(path, parameters, CancellationToken.None);
        }

        public virtual Task<object> GetTaskAsync(string path, object parameters, CancellationToken cancellationToken)
        {
            return ApiTaskAsync("GET", path, parameters, null, null, cancellationToken);
        }

        public virtual Task<object> PostTaskAsync(object parameters)
        {
            return PostTaskAsync(null, parameters, CancellationToken.None);
        }

        public virtual Task<object> PostTaskAsync(string path, object parameters)
        {
            return PostTaskAsync(path, parameters, CancellationToken.None);
        }

        public virtual Task<object> PostTaskAsync(string path, object parameters, CancellationToken cancellationToken)
        {
            return ApiTaskAsync("POST", path, parameters, null, null, cancellationToken);
        }

#if ASYNC_AWAIT
        public virtual Task<object> PostTaskAsync(string path, object parameters, object userToken, CancellationToken cancellationToken, IProgress<FacebookUploadProgressChangedEventArgs> uploadProgress)
        {
            return ApiTaskAsync("POST", path, parameters, null, userToken, cancellationToken, uploadProgress);
        }
#endif

        public virtual Task<object> DeleteTaskAsync(string path)
        {
            return DeleteTaskAsync(path, null, CancellationToken.None);
        }

        public virtual Task<object> DeleteTaskAsync(string path, object parameters)
        {
            return DeleteTaskAsync(path, parameters, CancellationToken.None);
        }

        public virtual Task<object> DeleteTaskAsync(string path, object parameters, CancellationToken cancellationToken)
        {
            return ApiTaskAsync("DELETE", path, parameters, null, null, cancellationToken);
        }
    }
}