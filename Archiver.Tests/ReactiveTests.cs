using Archiver.Util;
using Microsoft.Reactive.Testing;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Archiver.Tests
{
    public class ReactiveTests
    {
        private readonly ITestOutputHelper _output;

        public ReactiveTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task TestRetryWhen()
        {
            await Assert.ThrowsAsync<Exception>(async () => await Observable
                  .Throw<int>(new Exception("test"))
                  .RetryWhen(o => Observable
                     .Range(0, 3)
                     .Zip(o)
                     .SelectMany(tuple =>
                     {
                         if (tuple.First == 2)
                         {
                             throw tuple.Second;
                         }

                         return Observable.Timer(TimeSpan.FromSeconds(tuple.First));
                     })));
        }

        [Fact]
        public async Task TestRetryWhen2()
        {
            var source = Observable
                .Throw<int>(new Exception());

            var attempt = 0;
            await Observable
                .Defer(() => source.DelaySubscription(TimeSpan.FromSeconds(attempt++)))
                    .Retry(3);
        }

        [Fact]
        public void TestRepeatedExecution()
        {
            var taskFactory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(1));
            var scheduler = new TaskPoolScheduler(taskFactory);
            var tokenSource = new CancellationTokenSource();

            Enumerable.Range(1, 3)
                .Select(i => Observable.Timer(TimeSpan.FromMilliseconds(i * 100), scheduler).Do(_ =>
                {
                    _output.WriteLine($"Start {i}");
                    _output.WriteLine($"Stop {i}");
                }).Select(_ => i).Repeat(3))
                .ToObservable()
                .Merge()
                .Timestamp()
                .Do(i => _output.WriteLine("{0} @ {1}", i.Value, i.Timestamp.TimeOfDay))
                .Subscribe(tokenSource.Token);

            Thread.Sleep(300);
            tokenSource.Cancel();
        }

        [Fact]
        public async Task TestRepeat()
        {
            var ob = await Enumerable.Range(1, 3)
                .ToObservable()
                .Do(i =>
                {
                    _output.WriteLine(i.ToString());
                    Thread.Sleep(2000);
                })
                .Timestamp()
                .Do(t => _output.WriteLine(t.Timestamp.ToUnixTimeMilliseconds().ToString()))
                .Repeat(3);
        }

        [Fact]
        public async Task TestTimer()
        {
            var ob = await Observable.Create<string>(o =>
            {
                var delay = new TimeSpan(0, 0, 3);
                var timer = Observable
                    .Timer(delay)
                    .Subscribe(_ =>
                    {
                        o.OnNext("1");
                    }, _ => { }, () =>
                    {
                        o.OnCompleted();
                    });

                return Disposable.Create(() =>
                {
                    _output.WriteLine("unsubscribed");
                    timer.Dispose();
                });
            });
        }

        [Fact]
        public async Task TestChangeListener()
        {
            var st = Observable.Defer(() =>
            {
                Thread.Sleep(2000);
                _output.WriteLine("Element changed");

                return Observable.Return("2");
            });

            var op = await Observable.Return("1")
                .Merge(st)
                .Do(v => _output.WriteLine(v))
                .Select(p =>
                {
                    return Observable.Create<string>(o =>
                    {
                        var delay = new TimeSpan(0, 0, 3);
                        var timer = Observable
                            .Timer(delay)
                            .Subscribe(_ =>
                            {
                                _output.WriteLine($"Timer elapsed {p}");
                                o.OnNext(p);
                            }, _ => { }, () =>
                            {
                                o.OnCompleted();
                            });

                        return Disposable.Create(() =>
                        {
                            _output.WriteLine("unsubscribed");
                            timer.Dispose();
                        });
                    })
                    .Repeat(3);
                })
                .Switch();
        }

        [Fact]
        public async Task TestBehaviorSubject()
        {
            var subject = new BehaviorSubject<string>(null);
            subject.Subscribe(s => _output.WriteLine("Event"));

            subject.OnNext("test");
        }
    }
}
