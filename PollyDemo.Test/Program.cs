using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PollyDemo.Test
{
    /// <summary>
    /// 演示Polly库的使用
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            TestRetryByGoToLabel();
            TestRetryByLoop();
            TestRetryByPollyRetry();
            TestRetryByPollyWaitAndRetry();

            Console.ReadKey();
        }

        /// <summary>
        /// 通常的重试方法，使用goto+Label
        /// </summary>
        static void TestRetryByGoToLabel()
        {
            var retryCount = 3;
            RetryLabel:
            try
            {
                throwWebException();
            }
            catch (WebException ex)
            {
                if (--retryCount >= 0)
                {
                    Console.WriteLine("程序发生异常，重试一次!");
                    goto RetryLabel;
                }
            }

            Console.WriteLine("重试之后仍然失败，程序终止运行!");
        }

        /// <summary>
        /// 通常的重试方法，使用循环，比如while
        /// </summary>
        static void TestRetryByLoop()
        {
            var tryTimes = 1;
            while (tryTimes <= 3)
            {
                try
                {
                    throwWebException();
                }
                catch (WebException ex)
                {
                    Console.WriteLine("程序发生异常，重试一次!");
                    tryTimes++;
                }
            }

            Console.WriteLine("重试之后仍然失败，程序终止运行!");
        }

        /// <summary>
        /// 使用Polly的重试策略
        /// Retry
        /// </summary>
        static void TestRetryByPollyRetry()
        {
            try
            {
                var polly = Policy
                    .Handle<WebException>(ex => CanRetry(ex.Status))
                    .Or<DivideByZeroException>()
                    .Retry(3, (ex, count) =>
                {
                    Console.WriteLine("程序发生异常，重试第{0}次!", count);
                });
                polly.Execute(() =>
                {
                    throwWebException();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("重试之后仍然失败，程序终止运行!");
            }
        }

        /// <summary>
        /// 使用Polly的重试策略
        /// Wait and retry
        /// </summary>
        static void TestRetryByPollyWaitAndRetry()
        {
            try
            {
                var polly = Policy
                    .Handle<WebException>(ex => ex.Status == WebExceptionStatus.Timeout)
                    .Or<DivideByZeroException>()
                    .WaitAndRetry(new[]
                      {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(3)
                      }, (ex, timeSpan) =>
                      {
                          Console.WriteLine("程序发生异常，继续重试!");
                      });
                polly.Execute(() =>
                {
                    throwWebException();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("重试之后仍然失败，程序终止运行!");
            }
        }

        static void throwWebException()
        {
            throw new WebException("", WebExceptionStatus.Timeout);
        }

        /// <summary>
        /// 定义哪些网络异常需要重试
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        static bool CanRetry(WebExceptionStatus status)
        {
            return status == WebExceptionStatus.Timeout ||
                   status == WebExceptionStatus.ConnectionClosed ||
                   status == WebExceptionStatus.ConnectFailure ||
                   status == WebExceptionStatus.SendFailure ||
                   status == WebExceptionStatus.ReceiveFailure ||
                   status == WebExceptionStatus.RequestCanceled ||
                   status == WebExceptionStatus.KeepAliveFailure;
        }

    }
}
