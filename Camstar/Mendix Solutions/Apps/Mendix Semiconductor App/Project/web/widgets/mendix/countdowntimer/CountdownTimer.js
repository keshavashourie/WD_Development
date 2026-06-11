define(['exports', 'react'], (function (exports, x) { 'use strict';

  var G = typeof window == "undefined" ? x.useEffect : x.useLayoutEffect,
    I = ({
      isPlaying: o,
      duration: e,
      startAt: n = 0,
      updateInterval: t = 0,
      onComplete: s,
      onUpdate: r
    }) => {
      let [i, c] = x.useState(n),
        m = x.useRef(0),
        p = x.useRef(n),
        f = x.useRef(n * -1e3),
        u = x.useRef(null),
        a = x.useRef(null),
        h = x.useRef(null),
        w = g => {
          let l = g / 1e3;
          if (a.current === null) {
            a.current = l, u.current = requestAnimationFrame(w);
            return;
          }
          let d = l - a.current,
            C = m.current + d;
          a.current = l, m.current = C;
          let k = p.current + (t === 0 ? C : (C / t | 0) * t),
            R = p.current + C,
            v = typeof e == "number" && R >= e;
          c(v ? e : k), v || (u.current = requestAnimationFrame(w));
        },
        $ = () => {
          u.current && cancelAnimationFrame(u.current), h.current && clearTimeout(h.current), a.current = null;
        },
        y = x.useCallback(g => {
          $(), m.current = 0;
          let l = typeof g == "number" ? g : n;
          p.current = l, c(l), o && (u.current = requestAnimationFrame(w));
        }, [o, n]);
      return G(() => {
        if (r == null || r(i), e && i >= e) {
          f.current += e * 1e3;
          let {
            shouldRepeat: g = !1,
            delay: l = 0,
            newStartAt: d
          } = (s == null ? void 0 : s(f.current / 1e3)) || {};
          g && (h.current = setTimeout(() => y(d), l * 1e3));
        }
      }, [i, e]), G(() => (o && (u.current = requestAnimationFrame(w)), $), [o, e, t]), {
        elapsedTime: i,
        reset: y
      };
    };
  var A = (o, e, n) => {
      let t = o / 2,
        s = e / 2,
        r = t - s,
        i = 2 * r,
        c = n === "clockwise" ? "1,0" : "0,1",
        m = 2 * Math.PI * r;
      return {
        path: `m ${t},${s} a ${r},${r} 0 ${c} 0,${i} a ${r},${r} 0 ${c} 0,-${i}`,
        pathLength: m
      };
    },
    T = (o, e) => o === 0 || o === e ? 0 : typeof e == "number" ? o - e : 0,
    B = o => ({
      position: "relative",
      width: o,
      height: o
    }),
    P = {
      display: "flex",
      justifyContent: "center",
      alignItems: "center",
      position: "absolute",
      left: 0,
      top: 0,
      width: "100%",
      height: "100%"
    };
  var F = (o, e, n, t, s) => {
      if (t === 0) return e;
      let r = (s ? t - o : o) / t;
      return e + n * r;
    },
    W = o => {
      var e, n;
      return (n = (e = o.replace(/^#?([a-f\d])([a-f\d])([a-f\d])$/i, (t, s, r, i) => `#${s}${s}${r}${r}${i}${i}`).substring(1).match(/.{2}/g)) == null ? void 0 : e.map(t => parseInt(t, 16))) != null ? n : [];
    },
    j = (o, e) => {
      var u;
      let {
        colors: n,
        colorsTime: t,
        isSmoothColorTransition: s = !0
      } = o;
      if (typeof n == "string") return n;
      let r = (u = t == null ? void 0 : t.findIndex((a, h) => a >= e && e >= t[h + 1])) != null ? u : -1;
      if (!t || r === -1) return n[0];
      if (!s) return n[r];
      let i = t[r] - e,
        c = t[r] - t[r + 1],
        m = W(n[r]),
        p = W(n[r + 1]),
        f = !!o.isGrowing;
      return `rgb(${m.map((a, h) => F(i, a, p[h] - a, c, f) | 0).join(",")})`;
    },
    S = o => {
      let {
          duration: e,
          initialRemainingTime: n,
          updateInterval: t,
          size: s = 180,
          strokeWidth: r = 12,
          trailStrokeWidth: i,
          isPlaying: c = !1,
          isGrowing: m = !1,
          rotation: p = "clockwise",
          onComplete: f,
          onUpdate: u
        } = o,
        a = x.useRef(),
        h = Math.max(r, i != null ? i : 0),
        {
          path: w,
          pathLength: $
        } = A(s, h, p),
        {
          elapsedTime: y
        } = I({
          isPlaying: c,
          duration: e,
          startAt: T(e, n),
          updateInterval: t,
          onUpdate: typeof u == "function" ? l => {
            let d = Math.ceil(e - l);
            d !== a.current && (a.current = d, u(d));
          } : void 0,
          onComplete: typeof f == "function" ? l => {
            var R;
            let {
              shouldRepeat: d,
              delay: C,
              newInitialRemainingTime: k
            } = (R = f(l)) != null ? R : {};
            if (d) return {
              shouldRepeat: d,
              delay: C,
              newStartAt: T(e, k)
            };
          } : void 0
        }),
        g = e - y;
      return {
        elapsedTime: y,
        path: w,
        pathLength: $,
        remainingTime: Math.ceil(g),
        rotation: p,
        size: s,
        stroke: j(o, g),
        strokeDashoffset: F(y, 0, $, e, m),
        strokeWidth: r
      };
    };
  var D = o => {
    let {
        children: e,
        strokeLinecap: n,
        trailColor: t,
        trailStrokeWidth: s
      } = o,
      {
        path: r,
        pathLength: i,
        stroke: c,
        strokeDashoffset: m,
        remainingTime: p,
        elapsedTime: f,
        size: u,
        strokeWidth: a
      } = S(o);
    return x.createElement("div", {
      style: B(u)
    }, x.createElement("svg", {
      viewBox: `0 0 ${u} ${u}`,
      width: u,
      height: u,
      xmlns: "http://www.w3.org/2000/svg"
    }, x.createElement("path", {
      d: r,
      fill: "none",
      stroke: t != null ? t : "#d9d9d9",
      strokeWidth: s != null ? s : a
    }), x.createElement("path", {
      d: r,
      fill: "none",
      stroke: c,
      strokeLinecap: n != null ? n : "round",
      strokeWidth: a,
      strokeDasharray: i,
      strokeDashoffset: m
    })), typeof e == "function" && x.createElement("div", {
      style: P
    }, e({
      remainingTime: p,
      elapsedTime: f,
      color: c
    })));
  };
  D.displayName = "CountdownCircleTimer";

  const renderTime = (dimension, time, timeDuration, allocatedTime, isOvershoot) => {
    const duration = Math.round(allocatedTime);
    const minutes = Math.floor(time / 60).toString().padStart(2, '0');
    const seconds = Math.trunc(time % 60).toString().padStart(2, '0');

    //Only used for display.
    const formatedHours = Math.floor(duration / 3600).toString().padStart(2, '0');
    const formatedminutes = Math.floor(duration % 3600 / 60).toString().padStart(2, '0');
    return x.createElement("div", {
      className: "time-wrapper"
    }, x.createElement("div", {
      className: "time",
      style: {
        bottom: '90%'
      }
    }, x.createElement("div", {
      className: "task-name"
    }, "Task Time"), isOvershoot ? x.createElement("div", {
      className: "time-count time-count-overshoot"
    }, "+ ", minutes, ":", seconds, " min") : x.createElement("div", {
      className: "time-count"
    }, minutes, ":", seconds, " min"), timeDuration != 0 && allocatedTime != 0 || timeDuration == 0 && allocatedTime != 0 ? x.createElement("div", {
      className: "task-name"
    }, "/", formatedHours, ":", formatedminutes, " h") : null));
  };
  function CountdownTimerComponent({
    timeDuration,
    currentValue,
    startTime,
    onPauseClickAction,
    currentDateTime
  }) {
    const [isPlaying, setIsPlaying] = x.useState(true);
    const [time, setTime] = x.useState(timeDuration);
    const [timeStart, setTimeStart] = x.useState(startTime);
    const [key, setKey] = x.useState(0);
    const [xx, setXx] = x.useState(timeDuration);
    const [date, setDate] = x.useState(currentDateTime);
    const [defaultTime, setDefaultTime] = x.useState(Number.MAX_SAFE_INTEGER);
    const [allocatedTime, setAllocatedTime] = x.useState(timeDuration);
    const [countUpMode, setCountUpMode] = x.useState(false);
    const [isLoading, setIsLoading] = x.useState(true);
    x.useEffect(() => {
      setTime(timeDuration);
      setAllocatedTime(timeDuration);
    }, [timeDuration]);
    x.useEffect(() => {
      setTimeStart(startTime);
    }, [startTime]);
    x.useEffect(() => {
      setDate(currentDateTime);
      setKey(key + 1);
      setIsPlaying(true);
      setIsLoading(false);
      if (startTime > timeDuration)
        // In case of overshoot set the CountUpMode= true
        setCountUpMode(true);else setCountUpMode(false);
    }, [currentDateTime]);
    const handleComplete = () => {
      setCountUpMode(true);
      return {
        shouldRepeat: false
      }; // prevent auto-repeat
    };
    const PauseIcon = ({
      className = "timer-icon",
      color = 'currentColor'
    }) => x.createElement("svg", {
      xmlns: "http://www.w3.org/2000/svg",
      viewBox: "0 0 24 24",
      className: className
    }, x.createElement("path", {
      fill: "none",
      d: "M0 0h24v24H0z"
    }), x.createElement("path", {
      d: "M6 5h4v14H6V5zm8 0h4v14h-4V5z",
      fill: "transparent" // Make fill transparent
      ,
      stroke: color // Add stroke color
      ,
      strokeWidth: "1.5" // Add stroke width
    }));
    const StartIcon = ({
      className = "timer-icon",
      color = 'currentColor'
    }) => x.createElement("svg", {
      xmlns: "http://www.w3.org/2000/svg",
      viewBox: "0 0 24 24",
      className: className
    }, x.createElement("path", {
      fill: "none",
      d: "M0 0h24v24H0z"
    }), x.createElement("path", {
      d: "M8 5v14l11-7z",
      fill: "transparent" // Make fill transparent
      ,
      stroke: color // Add stroke color
      ,
      strokeWidth: "1.5" // Add stroke width
    }));
    const onPauseClickHandler = () => {
      if (isPlaying) {
        if (onPauseClickAction && onPauseClickAction.canExecute) {
          onPauseClickAction.execute();
        }
      }
      setIsPlaying(prev => !prev);
    };
    if (time == 0) {
      // When AllocationTime = 0
      return x.createElement("div", {
        className: "timer-container"
      }, x.createElement(D, {
        key: key,
        isPlaying: isPlaying,
        duration: defaultTime,
        trailColor: isPlaying ? "#0F7EA5" : "#888888",
        colors: isPlaying ? "#0F7EA5" : "#888888",
        size: 40,
        initialRemainingTime: defaultTime - timeStart,
        strokeWidth: 5,
        strokeLinecap: "square",
        onUpdate: elapsedTime => {
          // Update Mendix value on timer update
          if (currentValue?.status === "available") {
            const r = parseFloat(elapsedTime);
            const d = parseFloat(defaultTime);
            currentValue.setTextValue("" + ((d - r) / 86400).toFixed(8));
          }
        }
      }, ({
        elapsedTime
      }) => {
        return x.createElement("span", null, x.createElement("button", {
          onClick: onPauseClickHandler,
          style: {
            border: 'none',
            background: 'transparent',
            cursor: 'pointer'
          },
          className: "timer-button"
        }, isPlaying ? x.createElement(PauseIcon, {
          className: "timer-icon-button"
        }) : x.createElement(StartIcon, {
          className: "timer-icon-button"
        })), x.createElement("span", null, renderTime("seconds", elapsedTime, 0, allocatedTime, false)));
      }));
    } else {
      if (countUpMode) {
        // When AllocationTime > 0 && In case of overshoot
        return x.createElement("div", {
          className: "timer-container"
        }, x.createElement(D, {
          key: key,
          isPlaying: isPlaying,
          duration: defaultTime,
          trailColor: "#DC0000",
          colors: "#DC0000",
          size: 40,
          strokeWidth: 5,
          strokeLinecap: "square",
          initialRemainingTime: Math.round(startTime) > Math.round(time) ? defaultTime - timeStart : defaultTime,
          onUpdate: elapsedTime => {
            // Update Mendix value on timer update
            if (currentValue?.status === "available") {
              const r = parseFloat(elapsedTime);
              const d = parseFloat(defaultTime);
              if (Math.round(startTime) > Math.round(time)) currentValue.setTextValue("" + ((d - r) / 86400).toFixed(8));else currentValue.setTextValue("" + ((d - r) / 86400).toFixed(8));
            }
          }
        }, ({
          elapsedTime
        }) => {
          return x.createElement("span", null, x.createElement("button", {
            onClick: onPauseClickHandler,
            style: {
              border: 'none',
              background: 'transparent',
              cursor: 'pointer'
            },
            className: "timer-button"
          }, isPlaying ? x.createElement(PauseIcon, {
            className: "timer-icon-button"
          }) : x.createElement(StartIcon, {
            className: "timer-icon-button"
          })), x.createElement("span", null, renderTime("seconds", elapsedTime > time ? elapsedTime - time : elapsedTime, 0, allocatedTime, true)));
        }));
      } else {
        // When AllocationTime > 0
        return x.createElement("div", {
          className: "timer-container"
        }, x.createElement(D, {
          key: key,
          isPlaying: isPlaying,
          duration: time,
          colors: isPlaying ? ["#0F7EA5", "#0F7EA5", "#0F7EA5", "#0F7EA5"] : "#888888",
          colorsTime: [time, time / 2, 30, 0],
          size: 40,
          initialRemainingTime: timeStart,
          strokeWidth: 5,
          trailColor: isPlaying ? "#C0D3DB" : "#cccccc",
          strokeLinecap: "square",
          onComplete: handleComplete
        }, ({
          remainingTime
        }) => {
          if (xx !== remainingTime) {
            setTimeout(() => setXx(remainingTime), 0); // avoid setting state during render
            const r = parseFloat(remainingTime);
            const d = parseFloat(time);
            currentValue.setTextValue("" + ((d - r) / 86400).toFixed(8));
          }
          return x.createElement("span", null, x.createElement("button", {
            onClick: onPauseClickHandler,
            style: {
              border: 'none',
              background: 'transparent',
              cursor: 'pointer'
            },
            className: "timer-button"
          }, isPlaying ? x.createElement(PauseIcon, {
            className: "timer-icon-button"
          }) : x.createElement(StartIcon, {
            className: "timer-icon-button"
          })), x.createElement("span", null, renderTime("seconds", remainingTime, time, allocatedTime, false)));
        }));
      }
    }
  }

  // Helper function to compare Mendix-like objects (status and value)
  const areMendixPropsEqual = (prev, next) => {
    if (!prev || !next) return prev === next;
    if (prev.status !== next.status) return false;
    if (prev.value instanceof Date && next.value instanceof Date) {
      return prev.value.getTime() === next.value.getTime();
    }
    // For numbers, strings, etc.
    return prev.value === next.value;
  };

  // Custom comparison function for React.memo
  const areCountdownPropsEqual = (prevProps, nextProps) => {
    // Compare each Mendix-like prop using your helper
    const currentDateTimeChanged = !areMendixPropsEqual(prevProps.currentDateTime, nextProps.currentDateTime);
    // If true, it means a prop has changed, so return false (meaning re-render)
    if (currentDateTimeChanged) {
      return false;
    }
    // If no relevant prop has changed, return true (meaning DO NOT re-render)
    return true;
  };
  const CountdownTimer = x.memo(function CountdownTimer({
    timeDuration,
    currentValue,
    startTime,
    onPauseClickAction,
    currentDateTime
  }) {
    const [time, setTime] = x.useState(timeDuration);
    const [c, setC] = x.useState(currentValue);
    const [timeStart, setTimeStart] = x.useState(startTime);
    const [date, setDate] = x.useState(currentDateTime);
    x.useEffect(() => {
      // This will only run if timeDuration (prop) changes AND React.memo allowed a re-render
      setTime(timeDuration);
    }, [timeDuration]);
    x.useEffect(() => {
      setTimeStart(startTime);
    }, [startTime]);
    x.useEffect(() => {
      setC(currentValue);
    }, [currentValue]);
    x.useEffect(() => {
      setDate(currentDateTime);
    }, [currentDateTime]);
    if (time.status !== "available" || c.status !== "available" || timeStart.status !== "available" || date.status !== "available") {
      return x.createElement("div", null, "Loading...");
    } else {
      return x.createElement(CountdownTimerComponent, {
        timeDuration: time.value.toNumber(),
        currentValue: c,
        startTime: timeStart.value.toNumber(),
        onPauseClickAction: onPauseClickAction,
        currentDateTime: date.value
      });
    }
  }, areCountdownPropsEqual); // <-- Pass the custom comparison function here

  exports.CountdownTimer = CountdownTimer;

}));
//# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiQ291bnRkb3duVGltZXIuanMiLCJzb3VyY2VzIjpbIi4uLy4uLy4uLy4uLy4uL25vZGVfbW9kdWxlcy9yZWFjdC1jb3VudGRvd24tY2lyY2xlLXRpbWVyL2xpYi9pbmRleC5tb2R1bGUuanMiLCIuLi8uLi8uLi8uLi8uLi9zcmMvY29tcG9uZW50cy9Db3VudGRvd25UaW1lckNvbXBvbmVudC5qc3giLCIuLi8uLi8uLi8uLi8uLi9zcmMvQ291bnRkb3duVGltZXIuanN4Il0sInNvdXJjZXNDb250ZW50IjpbImltcG9ydCB4IGZyb21cInJlYWN0XCI7aW1wb3J0e3VzZVJlZiBhcyBVfWZyb21cInJlYWN0XCI7aW1wb3J0e3VzZVN0YXRlIGFzIEUsdXNlUmVmIGFzIGIsdXNlQ2FsbGJhY2sgYXMgcX1mcm9tXCJyZWFjdFwiO2ltcG9ydHt1c2VFZmZlY3QgYXMgTSx1c2VMYXlvdXRFZmZlY3QgYXMgTH1mcm9tXCJyZWFjdFwiO3ZhciBHPXR5cGVvZiB3aW5kb3c9PVwidW5kZWZpbmVkXCI/TTpMLEk9KHtpc1BsYXlpbmc6byxkdXJhdGlvbjplLHN0YXJ0QXQ6bj0wLHVwZGF0ZUludGVydmFsOnQ9MCxvbkNvbXBsZXRlOnMsb25VcGRhdGU6cn0pPT57bGV0W2ksY109RShuKSxtPWIoMCkscD1iKG4pLGY9YihuKi0xZTMpLHU9YihudWxsKSxhPWIobnVsbCksaD1iKG51bGwpLHc9Zz0+e2xldCBsPWcvMWUzO2lmKGEuY3VycmVudD09PW51bGwpe2EuY3VycmVudD1sLHUuY3VycmVudD1yZXF1ZXN0QW5pbWF0aW9uRnJhbWUodyk7cmV0dXJufWxldCBkPWwtYS5jdXJyZW50LEM9bS5jdXJyZW50K2Q7YS5jdXJyZW50PWwsbS5jdXJyZW50PUM7bGV0IGs9cC5jdXJyZW50Kyh0PT09MD9DOihDL3R8MCkqdCksUj1wLmN1cnJlbnQrQyx2PXR5cGVvZiBlPT1cIm51bWJlclwiJiZSPj1lO2Modj9lOmspLHZ8fCh1LmN1cnJlbnQ9cmVxdWVzdEFuaW1hdGlvbkZyYW1lKHcpKX0sJD0oKT0+e3UuY3VycmVudCYmY2FuY2VsQW5pbWF0aW9uRnJhbWUodS5jdXJyZW50KSxoLmN1cnJlbnQmJmNsZWFyVGltZW91dChoLmN1cnJlbnQpLGEuY3VycmVudD1udWxsfSx5PXEoZz0+eyQoKSxtLmN1cnJlbnQ9MDtsZXQgbD10eXBlb2YgZz09XCJudW1iZXJcIj9nOm47cC5jdXJyZW50PWwsYyhsKSxvJiYodS5jdXJyZW50PXJlcXVlc3RBbmltYXRpb25GcmFtZSh3KSl9LFtvLG5dKTtyZXR1cm4gRygoKT0+e2lmKHI9PW51bGx8fHIoaSksZSYmaT49ZSl7Zi5jdXJyZW50Kz1lKjFlMztsZXR7c2hvdWxkUmVwZWF0Omc9ITEsZGVsYXk6bD0wLG5ld1N0YXJ0QXQ6ZH09KHM9PW51bGw/dm9pZCAwOnMoZi5jdXJyZW50LzFlMykpfHx7fTtnJiYoaC5jdXJyZW50PXNldFRpbWVvdXQoKCk9PnkoZCksbCoxZTMpKX19LFtpLGVdKSxHKCgpPT4obyYmKHUuY3VycmVudD1yZXF1ZXN0QW5pbWF0aW9uRnJhbWUodykpLCQpLFtvLGUsdF0pLHtlbGFwc2VkVGltZTppLHJlc2V0Onl9fTt2YXIgQT0obyxlLG4pPT57bGV0IHQ9by8yLHM9ZS8yLHI9dC1zLGk9MipyLGM9bj09PVwiY2xvY2t3aXNlXCI/XCIxLDBcIjpcIjAsMVwiLG09MipNYXRoLlBJKnI7cmV0dXJue3BhdGg6YG0gJHt0fSwke3N9IGEgJHtyfSwke3J9IDAgJHtjfSAwLCR7aX0gYSAke3J9LCR7cn0gMCAke2N9IDAsLSR7aX1gLHBhdGhMZW5ndGg6bX19LFQ9KG8sZSk9Pm89PT0wfHxvPT09ZT8wOnR5cGVvZiBlPT1cIm51bWJlclwiP28tZTowLEI9bz0+KHtwb3NpdGlvbjpcInJlbGF0aXZlXCIsd2lkdGg6byxoZWlnaHQ6b30pLFA9e2Rpc3BsYXk6XCJmbGV4XCIsanVzdGlmeUNvbnRlbnQ6XCJjZW50ZXJcIixhbGlnbkl0ZW1zOlwiY2VudGVyXCIscG9zaXRpb246XCJhYnNvbHV0ZVwiLGxlZnQ6MCx0b3A6MCx3aWR0aDpcIjEwMCVcIixoZWlnaHQ6XCIxMDAlXCJ9O3ZhciBGPShvLGUsbix0LHMpPT57aWYodD09PTApcmV0dXJuIGU7bGV0IHI9KHM/dC1vOm8pL3Q7cmV0dXJuIGUrbipyfSxXPW89Pnt2YXIgZSxuO3JldHVybihuPShlPW8ucmVwbGFjZSgvXiM/KFthLWZcXGRdKShbYS1mXFxkXSkoW2EtZlxcZF0pJC9pLCh0LHMscixpKT0+YCMke3N9JHtzfSR7cn0ke3J9JHtpfSR7aX1gKS5zdWJzdHJpbmcoMSkubWF0Y2goLy57Mn0vZykpPT1udWxsP3ZvaWQgMDplLm1hcCh0PT5wYXJzZUludCh0LDE2KSkpIT1udWxsP246W119LGo9KG8sZSk9Pnt2YXIgdTtsZXR7Y29sb3JzOm4sY29sb3JzVGltZTp0LGlzU21vb3RoQ29sb3JUcmFuc2l0aW9uOnM9ITB9PW87aWYodHlwZW9mIG49PVwic3RyaW5nXCIpcmV0dXJuIG47bGV0IHI9KHU9dD09bnVsbD92b2lkIDA6dC5maW5kSW5kZXgoKGEsaCk9PmE+PWUmJmU+PXRbaCsxXSkpIT1udWxsP3U6LTE7aWYoIXR8fHI9PT0tMSlyZXR1cm4gblswXTtpZighcylyZXR1cm4gbltyXTtsZXQgaT10W3JdLWUsYz10W3JdLXRbcisxXSxtPVcobltyXSkscD1XKG5bcisxXSksZj0hIW8uaXNHcm93aW5nO3JldHVybmByZ2IoJHttLm1hcCgoYSxoKT0+RihpLGEscFtoXS1hLGMsZil8MCkuam9pbihcIixcIil9KWB9LFM9bz0+e2xldHtkdXJhdGlvbjplLGluaXRpYWxSZW1haW5pbmdUaW1lOm4sdXBkYXRlSW50ZXJ2YWw6dCxzaXplOnM9MTgwLHN0cm9rZVdpZHRoOnI9MTIsdHJhaWxTdHJva2VXaWR0aDppLGlzUGxheWluZzpjPSExLGlzR3Jvd2luZzptPSExLHJvdGF0aW9uOnA9XCJjbG9ja3dpc2VcIixvbkNvbXBsZXRlOmYsb25VcGRhdGU6dX09byxhPVUoKSxoPU1hdGgubWF4KHIsaSE9bnVsbD9pOjApLHtwYXRoOncscGF0aExlbmd0aDokfT1BKHMsaCxwKSx7ZWxhcHNlZFRpbWU6eX09SSh7aXNQbGF5aW5nOmMsZHVyYXRpb246ZSxzdGFydEF0OlQoZSxuKSx1cGRhdGVJbnRlcnZhbDp0LG9uVXBkYXRlOnR5cGVvZiB1PT1cImZ1bmN0aW9uXCI/bD0+e2xldCBkPU1hdGguY2VpbChlLWwpO2QhPT1hLmN1cnJlbnQmJihhLmN1cnJlbnQ9ZCx1KGQpKX06dm9pZCAwLG9uQ29tcGxldGU6dHlwZW9mIGY9PVwiZnVuY3Rpb25cIj9sPT57dmFyIFI7bGV0e3Nob3VsZFJlcGVhdDpkLGRlbGF5OkMsbmV3SW5pdGlhbFJlbWFpbmluZ1RpbWU6a309KFI9ZihsKSkhPW51bGw/Ujp7fTtpZihkKXJldHVybntzaG91bGRSZXBlYXQ6ZCxkZWxheTpDLG5ld1N0YXJ0QXQ6VChlLGspfX06dm9pZCAwfSksZz1lLXk7cmV0dXJue2VsYXBzZWRUaW1lOnkscGF0aDp3LHBhdGhMZW5ndGg6JCxyZW1haW5pbmdUaW1lOk1hdGguY2VpbChnKSxyb3RhdGlvbjpwLHNpemU6cyxzdHJva2U6aihvLGcpLHN0cm9rZURhc2hvZmZzZXQ6Rih5LDAsJCxlLG0pLHN0cm9rZVdpZHRoOnJ9fTt2YXIgRD1vPT57bGV0e2NoaWxkcmVuOmUsc3Ryb2tlTGluZWNhcDpuLHRyYWlsQ29sb3I6dCx0cmFpbFN0cm9rZVdpZHRoOnN9PW8se3BhdGg6cixwYXRoTGVuZ3RoOmksc3Ryb2tlOmMsc3Ryb2tlRGFzaG9mZnNldDptLHJlbWFpbmluZ1RpbWU6cCxlbGFwc2VkVGltZTpmLHNpemU6dSxzdHJva2VXaWR0aDphfT1TKG8pO3JldHVybiB4LmNyZWF0ZUVsZW1lbnQoXCJkaXZcIix7c3R5bGU6Qih1KX0seC5jcmVhdGVFbGVtZW50KFwic3ZnXCIse3ZpZXdCb3g6YDAgMCAke3V9ICR7dX1gLHdpZHRoOnUsaGVpZ2h0OnUseG1sbnM6XCJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2Z1wifSx4LmNyZWF0ZUVsZW1lbnQoXCJwYXRoXCIse2Q6cixmaWxsOlwibm9uZVwiLHN0cm9rZTp0IT1udWxsP3Q6XCIjZDlkOWQ5XCIsc3Ryb2tlV2lkdGg6cyE9bnVsbD9zOmF9KSx4LmNyZWF0ZUVsZW1lbnQoXCJwYXRoXCIse2Q6cixmaWxsOlwibm9uZVwiLHN0cm9rZTpjLHN0cm9rZUxpbmVjYXA6biE9bnVsbD9uOlwicm91bmRcIixzdHJva2VXaWR0aDphLHN0cm9rZURhc2hhcnJheTppLHN0cm9rZURhc2hvZmZzZXQ6bX0pKSx0eXBlb2YgZT09XCJmdW5jdGlvblwiJiZ4LmNyZWF0ZUVsZW1lbnQoXCJkaXZcIix7c3R5bGU6UH0sZSh7cmVtYWluaW5nVGltZTpwLGVsYXBzZWRUaW1lOmYsY29sb3I6Y30pKSl9O0QuZGlzcGxheU5hbWU9XCJDb3VudGRvd25DaXJjbGVUaW1lclwiO2V4cG9ydHtEIGFzIENvdW50ZG93bkNpcmNsZVRpbWVyLFMgYXMgdXNlQ291bnRkb3dufTtcbiIsImltcG9ydCB7IGNyZWF0ZUVsZW1lbnQsIHVzZVN0YXRlLCB1c2VFZmZlY3QgfSBmcm9tIFwicmVhY3RcIjtcbmltcG9ydCB7IENvdW50ZG93bkNpcmNsZVRpbWVyIH0gZnJvbSAncmVhY3QtY291bnRkb3duLWNpcmNsZS10aW1lcidcblxuY29uc3QgcmVuZGVyVGltZSA9IChkaW1lbnNpb24sIHRpbWUsIHRpbWVEdXJhdGlvbiwgYWxsb2NhdGVkVGltZSwgaXNPdmVyc2hvb3QpID0+IHtcbiAgY29uc3QgZHVyYXRpb24gPSBNYXRoLnJvdW5kKGFsbG9jYXRlZFRpbWUpO1xuIFxuICBjb25zdCBtaW51dGVzID0gTWF0aC5mbG9vcih0aW1lIC8gNjApLnRvU3RyaW5nKCkucGFkU3RhcnQoMiwgJzAnKTtcbiAgY29uc3Qgc2Vjb25kcyA9IE1hdGgudHJ1bmMoKHRpbWUgJSA2MCkpLnRvU3RyaW5nKCkucGFkU3RhcnQoMiwgJzAnKTs7XG4gXG4gIC8vT25seSB1c2VkIGZvciBkaXNwbGF5LlxuICBjb25zdCBmb3JtYXRlZEhvdXJzID0gTWF0aC5mbG9vcihkdXJhdGlvbiAvIDM2MDApLnRvU3RyaW5nKCkucGFkU3RhcnQoMiwgJzAnKTtcbiAgY29uc3QgZm9ybWF0ZWRtaW51dGVzID0gTWF0aC5mbG9vcigoZHVyYXRpb24gJSAzNjAwKSAvIDYwKS50b1N0cmluZygpLnBhZFN0YXJ0KDIsICcwJyk7XG5cbiBcbiAgcmV0dXJuIChcbiAgICAgIDxkaXYgY2xhc3NOYW1lPVwidGltZS13cmFwcGVyXCI+XG4gICAgICA8ZGl2IGNsYXNzTmFtZT1cInRpbWVcIiBzdHlsZT17eyBib3R0b206ICc5MCUnIH19PlxuICAgICAgICA8ZGl2IGNsYXNzTmFtZT1cInRhc2stbmFtZVwiPlRhc2sgVGltZTwvZGl2PlxuICAgICAgICB7aXNPdmVyc2hvb3Q/KDxkaXYgY2xhc3NOYW1lPVwidGltZS1jb3VudCB0aW1lLWNvdW50LW92ZXJzaG9vdFwiPisge21pbnV0ZXN9OntzZWNvbmRzfSBtaW48L2Rpdj4pOig8ZGl2IGNsYXNzTmFtZT1cInRpbWUtY291bnRcIj57bWludXRlc306e3NlY29uZHN9IG1pbjwvZGl2Pil9XG4gICAgICAgIHsoKHRpbWVEdXJhdGlvbiAhPSAwICYmIGFsbG9jYXRlZFRpbWUgIT0gMCkgfHwgKHRpbWVEdXJhdGlvbiA9PSAwICYmIGFsbG9jYXRlZFRpbWUgIT0gMCkpID8gKDxkaXYgY2xhc3NOYW1lPVwidGFzay1uYW1lXCI+L3tmb3JtYXRlZEhvdXJzfTp7Zm9ybWF0ZWRtaW51dGVzfSBoPC9kaXY+KSA6IG51bGx9XG4gICAgICA8L2Rpdj5cbiAgICA8L2Rpdj5cbiAgKTtcbn07XG5cblxuXG5leHBvcnQgZnVuY3Rpb24gQ291bnRkb3duVGltZXJDb21wb25lbnQoeyB0aW1lRHVyYXRpb24sIGN1cnJlbnRWYWx1ZSwgc3RhcnRUaW1lLCBvblBhdXNlQ2xpY2tBY3Rpb24sIGN1cnJlbnREYXRlVGltZSB9KSB7XG5cbiAgY29uc3QgW2lzUGxheWluZywgc2V0SXNQbGF5aW5nXSA9IHVzZVN0YXRlKHRydWUpO1xuICBjb25zdCBbdGltZSwgc2V0VGltZV0gPSB1c2VTdGF0ZSh0aW1lRHVyYXRpb24pO1xuICBjb25zdCBbdGltZVN0YXJ0LCBzZXRUaW1lU3RhcnRdID0gdXNlU3RhdGUoc3RhcnRUaW1lKTtcbiAgY29uc3QgW2tleSwgc2V0S2V5XSA9IHVzZVN0YXRlKDApO1xuICBjb25zdCBbeHgsIHNldFh4XSA9IHVzZVN0YXRlKHRpbWVEdXJhdGlvbik7XG4gIGNvbnN0IFtkYXRlLCBzZXREYXRlXSA9IHVzZVN0YXRlKGN1cnJlbnREYXRlVGltZSk7XG4gIGNvbnN0IFtkZWZhdWx0VGltZSwgc2V0RGVmYXVsdFRpbWVdID0gdXNlU3RhdGUoTnVtYmVyLk1BWF9TQUZFX0lOVEVHRVIpO1xuICBjb25zdCBbYWxsb2NhdGVkVGltZSwgc2V0QWxsb2NhdGVkVGltZV0gPSB1c2VTdGF0ZSh0aW1lRHVyYXRpb24pO1xuICBjb25zdCBbY291bnRVcE1vZGUsIHNldENvdW50VXBNb2RlXSA9IHVzZVN0YXRlKGZhbHNlKTtcbiAgY29uc3QgW2lzTG9hZGluZywgc2V0SXNMb2FkaW5nXSA9IHVzZVN0YXRlKHRydWUpO1xuXG4gIHVzZUVmZmVjdCgoKSA9PiB7XG4gICAgc2V0VGltZSh0aW1lRHVyYXRpb24pXG4gICAgc2V0QWxsb2NhdGVkVGltZSh0aW1lRHVyYXRpb24pXG4gIH0sIFt0aW1lRHVyYXRpb25dKTtcblxuICB1c2VFZmZlY3QoKCkgPT4ge1xuICAgIHNldFRpbWVTdGFydChzdGFydFRpbWUpXG4gIH0sIFtzdGFydFRpbWVdKTtcblxuICB1c2VFZmZlY3QoKCkgPT4ge1xuICAgIHNldERhdGUoY3VycmVudERhdGVUaW1lKVxuICAgIHNldEtleShrZXkgKyAxKVxuICAgIHNldElzUGxheWluZyh0cnVlKVxuICAgIHNldElzTG9hZGluZyhmYWxzZSk7XG4gICAgaWYgKHN0YXJ0VGltZSA+IHRpbWVEdXJhdGlvbikgIC8vIEluIGNhc2Ugb2Ygb3ZlcnNob290IHNldCB0aGUgQ291bnRVcE1vZGU9IHRydWVcbiAgICAgIHNldENvdW50VXBNb2RlKHRydWUpXG4gICAgZWxzZSBcbiAgICAgIHNldENvdW50VXBNb2RlKGZhbHNlKVxuICB9LCBbY3VycmVudERhdGVUaW1lXSk7XG5cbiAgY29uc3QgaGFuZGxlQ29tcGxldGUgPSAoKSA9PiB7XG4gICAgc2V0Q291bnRVcE1vZGUodHJ1ZSk7XG4gICAgcmV0dXJuIHsgc2hvdWxkUmVwZWF0OiBmYWxzZSB9OyAvLyBwcmV2ZW50IGF1dG8tcmVwZWF0XG4gIH07XG5cbiAgY29uc3QgUGF1c2VJY29uID0gKHsgY2xhc3NOYW1lID0gXCJ0aW1lci1pY29uXCIsIGNvbG9yID0gJ2N1cnJlbnRDb2xvcicgfSkgPT4gKFxuICAgIDxzdmcgeG1sbnM9XCJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2Z1wiIHZpZXdCb3g9XCIwIDAgMjQgMjRcIiBjbGFzc05hbWU9e2NsYXNzTmFtZX0+XG4gICAgICA8cGF0aCBmaWxsPVwibm9uZVwiIGQ9XCJNMCAwaDI0djI0SDB6XCIgLz5cbiAgICAgICBcbiAgICAgIDxwYXRoIGQ9XCJNNiA1aDR2MTRINlY1em04IDBoNHYxNGgtNFY1elwiXG4gICAgICAgIGZpbGw9XCJ0cmFuc3BhcmVudFwiICAvLyBNYWtlIGZpbGwgdHJhbnNwYXJlbnRcbiAgICAgICAgc3Ryb2tlPXtjb2xvcn0gICAgIC8vIEFkZCBzdHJva2UgY29sb3JcbiAgICAgICAgc3Ryb2tlV2lkdGg9XCIxLjVcIiAgLy8gQWRkIHN0cm9rZSB3aWR0aFxuICAgICAgLz5cbiAgICA8L3N2Zz5cbiAgKTtcblxuICBjb25zdCBTdGFydEljb24gPSAoeyBjbGFzc05hbWUgPSBcInRpbWVyLWljb25cIiwgY29sb3IgPSAnY3VycmVudENvbG9yJyB9KSA9PiAoXG4gICAgPHN2ZyB4bWxucz1cImh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnXCIgdmlld0JveD1cIjAgMCAyNCAyNFwiIGNsYXNzTmFtZT17Y2xhc3NOYW1lfT5cbiAgICAgIFxuICAgICAgPHBhdGggZmlsbD1cIm5vbmVcIiBkPVwiTTAgMGgyNHYyNEgwelwiIC8+XG4gICAgICAgIDxwYXRoIGQ9XCJNOCA1djE0bDExLTd6XCJcbiAgICAgICAgZmlsbD1cInRyYW5zcGFyZW50XCIgIC8vIE1ha2UgZmlsbCB0cmFuc3BhcmVudFxuICAgICAgICBzdHJva2U9e2NvbG9yfSAgICAgLy8gQWRkIHN0cm9rZSBjb2xvclxuICAgICAgICBzdHJva2VXaWR0aD1cIjEuNVwiICAvLyBBZGQgc3Ryb2tlIHdpZHRoXG5cbiAgICAgIC8+XG4gICAgPC9zdmc+XG4gICk7XG5cbiAgY29uc3Qgb25QYXVzZUNsaWNrSGFuZGxlciA9ICgpID0+IHtcbiAgICBpZiAoaXNQbGF5aW5nKSB7XG4gICAgICBpZiAob25QYXVzZUNsaWNrQWN0aW9uICYmIG9uUGF1c2VDbGlja0FjdGlvbi5jYW5FeGVjdXRlKSB7XG4gICAgICAgIG9uUGF1c2VDbGlja0FjdGlvbi5leGVjdXRlKCk7XG4gICAgICB9XG4gICAgfVxuICAgIHNldElzUGxheWluZygocHJldikgPT4gIXByZXYpO1xuICB9XG5cbiAgaWYgKHRpbWUgPT0gMCkgeyAgICAvLyBXaGVuIEFsbG9jYXRpb25UaW1lID0gMFxuICAgIHJldHVybiAoXG4gICAgICA8ZGl2IGNsYXNzTmFtZT1cInRpbWVyLWNvbnRhaW5lclwiPlxuICAgICAgICA8Q291bnRkb3duQ2lyY2xlVGltZXJcbiAgICAgICAgICBrZXk9e2tleX1cbiAgICAgICAgICBpc1BsYXlpbmc9e2lzUGxheWluZ31cbiAgICAgICAgICBkdXJhdGlvbj17ZGVmYXVsdFRpbWV9XG4gICAgICAgICAgdHJhaWxDb2xvcj17aXNQbGF5aW5nID8gXCIjMEY3RUE1XCIgOlwiIzg4ODg4OFwifVxuICAgICAgICAgIGNvbG9ycz17aXNQbGF5aW5nID8gXCIjMEY3RUE1XCIgOlwiIzg4ODg4OFwifVxuICAgICAgICAgIHNpemU9ezQwfVxuICAgICAgICAgIGluaXRpYWxSZW1haW5pbmdUaW1lPXtkZWZhdWx0VGltZSAtIHRpbWVTdGFydH1cbiAgICAgICAgICBzdHJva2VXaWR0aD17NX1cbiAgICAgICAgICBzdHJva2VMaW5lY2FwPVwic3F1YXJlXCJcbiAgICAgICAgICBvblVwZGF0ZT17KGVsYXBzZWRUaW1lKSA9PiB7XG4gICAgICAgICAgICAvLyBVcGRhdGUgTWVuZGl4IHZhbHVlIG9uIHRpbWVyIHVwZGF0ZVxuICAgICAgICAgICAgaWYgKGN1cnJlbnRWYWx1ZT8uc3RhdHVzID09PSBcImF2YWlsYWJsZVwiKSB7XG4gICAgICAgICAgICAgIGNvbnN0IHIgPSBwYXJzZUZsb2F0KGVsYXBzZWRUaW1lKTtcbiAgICAgICAgICAgICAgY29uc3QgZCA9IHBhcnNlRmxvYXQoZGVmYXVsdFRpbWUpO1xuICAgICAgICAgICAgICBjdXJyZW50VmFsdWUuc2V0VGV4dFZhbHVlKFwiXCIgKyAoKGQtcikvODY0MDApLnRvRml4ZWQoOCkpO1xuICAgICAgICAgICAgfVxuICAgICAgICAgIH19XG4gICAgICAgID5cblxuICAgICAgICAgIHsoeyBlbGFwc2VkVGltZSB9KSA9PiB7XG5cbiAgICAgICAgICAgIHJldHVybiAoXG4gICAgICAgICAgICAgIDxzcGFuPlxuICAgICAgICAgICAgICAgIDxidXR0b24gb25DbGljaz17b25QYXVzZUNsaWNrSGFuZGxlcn0gc3R5bGU9e3sgYm9yZGVyOiAnbm9uZScsIGJhY2tncm91bmQ6ICd0cmFuc3BhcmVudCcsIGN1cnNvcjogJ3BvaW50ZXInIH19IGNsYXNzTmFtZT1cInRpbWVyLWJ1dHRvblwiPlxuICAgICAgICAgICAgICAgICAge2lzUGxheWluZyA/XG4gICAgICAgICAgICAgICAgICAgICg8UGF1c2VJY29uIGNsYXNzTmFtZT1cInRpbWVyLWljb24tYnV0dG9uXCIgLz4pXG4gICAgICAgICAgICAgICAgICAgIDpcbiAgICAgICAgICAgICAgICAgICAgKDxTdGFydEljb24gY2xhc3NOYW1lPVwidGltZXItaWNvbi1idXR0b25cIiAvPil9XG4gICAgICAgICAgICAgICAgPC9idXR0b24+XG5cbiAgICAgICAgICAgICAgICA8c3BhbiA+XG4gICAgICAgICAgICAgICAgICB7cmVuZGVyVGltZShcInNlY29uZHNcIiwgZWxhcHNlZFRpbWUsIDAsIGFsbG9jYXRlZFRpbWUsIGZhbHNlKX1cbiAgICAgICAgICAgICAgICA8L3NwYW4+XG4gICAgICAgICAgICAgIDwvc3Bhbj5cbiAgICAgICAgICAgICk7XG4gICAgICAgICAgfX1cbiAgICAgICAgPC9Db3VudGRvd25DaXJjbGVUaW1lcj5cbiAgICAgIDwvZGl2PlxuICAgIClcbiAgfVxuICBlbHNlIHtcbiAgICBpZiAoY291bnRVcE1vZGUpIHsgICAvLyBXaGVuIEFsbG9jYXRpb25UaW1lID4gMCAmJiBJbiBjYXNlIG9mIG92ZXJzaG9vdFxuICAgICAgcmV0dXJuIChcbiAgICAgICAgPGRpdiBjbGFzc05hbWU9XCJ0aW1lci1jb250YWluZXJcIj5cbiAgICAgICAgICA8Q291bnRkb3duQ2lyY2xlVGltZXJcbiAgICAgICAgICAgIGtleT17a2V5fVxuICAgICAgICAgICAgaXNQbGF5aW5nPXtpc1BsYXlpbmd9XG4gICAgICAgICAgICBkdXJhdGlvbj17ZGVmYXVsdFRpbWV9XG4gICAgICAgICAgICB0cmFpbENvbG9yPXtcIiNEQzAwMDBcIn1cbiAgICAgICAgICAgIGNvbG9ycz17XCIjREMwMDAwXCJ9XG4gICAgICAgICAgICBzaXplPXs0MH1cbiAgICAgICAgICAgIHN0cm9rZVdpZHRoPXs1fVxuICAgICAgICAgICAgc3Ryb2tlTGluZWNhcD1cInNxdWFyZVwiXG4gICAgICAgICAgICBpbml0aWFsUmVtYWluaW5nVGltZT17KE1hdGgucm91bmQoc3RhcnRUaW1lKSA+IE1hdGgucm91bmQodGltZSkpIFxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgID8gKGRlZmF1bHRUaW1lIC0gdGltZVN0YXJ0KSBcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA6IGRlZmF1bHRUaW1lfVxuICAgICAgICAgICAgb25VcGRhdGU9eyhlbGFwc2VkVGltZSkgPT4ge1xuICAgICAgICAgICAgICAvLyBVcGRhdGUgTWVuZGl4IHZhbHVlIG9uIHRpbWVyIHVwZGF0ZVxuICAgICAgICAgICAgICBpZiAoY3VycmVudFZhbHVlPy5zdGF0dXMgPT09IFwiYXZhaWxhYmxlXCIpIHtcbiAgICAgICAgICAgICAgICBjb25zdCB0ID0gcGFyc2VGbG9hdCh0aW1lKTtcbiAgICAgICAgICAgICAgICBjb25zdCByID0gcGFyc2VGbG9hdChlbGFwc2VkVGltZSk7XG4gICAgICAgICAgICAgICAgY29uc3QgZCA9IHBhcnNlRmxvYXQoZGVmYXVsdFRpbWUpO1xuICAgICAgICAgICAgICAgIGlmKE1hdGgucm91bmQoc3RhcnRUaW1lKSA+IE1hdGgucm91bmQodGltZSkpXG4gICAgICAgICAgICAgICAgICAgY3VycmVudFZhbHVlLnNldFRleHRWYWx1ZShcIlwiICsgKChkLXIpLzg2NDAwKS50b0ZpeGVkKDgpKTtcbiAgICAgICAgICAgICAgICBlbHNlXG4gICAgICAgICAgICAgICAgICBjdXJyZW50VmFsdWUuc2V0VGV4dFZhbHVlKFwiXCIgKyAoKGQtcikvODY0MDApLnRvRml4ZWQoOCkpO1xuICAgICAgICAgICAgICB9XG4gICAgICAgICAgICB9fVxuICAgICAgICAgID5cblxuICAgICAgICAgICAgeyh7IGVsYXBzZWRUaW1lIH0pID0+IHtcblxuICAgICAgICAgICAgICByZXR1cm4gKFxuICAgICAgICAgICAgICAgIDxzcGFuPlxuICAgICAgICAgICAgICAgICAgPGJ1dHRvbiBvbkNsaWNrPXtvblBhdXNlQ2xpY2tIYW5kbGVyfSBzdHlsZT17eyBib3JkZXI6ICdub25lJywgYmFja2dyb3VuZDogJ3RyYW5zcGFyZW50JywgY3Vyc29yOiAncG9pbnRlcicgfX0gY2xhc3NOYW1lPVwidGltZXItYnV0dG9uXCI+XG4gICAgICAgICAgICAgICAgICAgIHtpc1BsYXlpbmcgP1xuICAgICAgICAgICAgICAgICAgICAgICg8UGF1c2VJY29uIGNsYXNzTmFtZT1cInRpbWVyLWljb24tYnV0dG9uXCIgLz4pXG4gICAgICAgICAgICAgICAgICAgICAgOlxuICAgICAgICAgICAgICAgICAgICAgICg8U3RhcnRJY29uIGNsYXNzTmFtZT1cInRpbWVyLWljb24tYnV0dG9uXCIgLz4pfVxuICAgICAgICAgICAgICAgICAgPC9idXR0b24+XG5cbiAgICAgICAgICAgICAgICAgIDxzcGFuID5cbiAgICAgICAgICAgICAgICAgICAge3JlbmRlclRpbWUoXCJzZWNvbmRzXCIsIChlbGFwc2VkVGltZSA+IHRpbWUpPyhlbGFwc2VkVGltZS10aW1lKTplbGFwc2VkVGltZSwgMCwgYWxsb2NhdGVkVGltZSx0cnVlKX1cbiAgICAgICAgICAgICAgICAgIDwvc3Bhbj5cbiAgICAgICAgICAgICAgICA8L3NwYW4+XG4gICAgICAgICAgICAgICk7XG4gICAgICAgICAgICB9fVxuICAgICAgICAgIDwvQ291bnRkb3duQ2lyY2xlVGltZXI+XG4gICAgICAgIDwvZGl2PlxuICAgICAgKVxuICAgIH1cbiAgICBlbHNlIHsgICAgICAgICAgICAgIC8vIFdoZW4gQWxsb2NhdGlvblRpbWUgPiAwXG4gICAgICByZXR1cm4gKFxuICAgICAgICA8ZGl2IGNsYXNzTmFtZT1cInRpbWVyLWNvbnRhaW5lclwiPlxuICAgICAgICAgIDxDb3VudGRvd25DaXJjbGVUaW1lclxuICAgICAgICAgICAga2V5PXtrZXl9XG4gICAgICAgICAgICBpc1BsYXlpbmc9e2lzUGxheWluZ31cbiAgICAgICAgICAgIGR1cmF0aW9uPXt0aW1lfVxuICAgICAgICAgICAgY29sb3JzPXtpc1BsYXlpbmcgPyBbXCIjMEY3RUE1XCIsIFwiIzBGN0VBNVwiLCBcIiMwRjdFQTVcIiwgXCIjMEY3RUE1XCJdIDogXCIjODg4ODg4XCJ9XG4gICAgICAgICAgICBjb2xvcnNUaW1lPXtbdGltZSwgdGltZSAvIDIsIDMwLCAwXX1cbiAgICAgICAgICAgIHNpemU9ezQwfVxuICAgICAgICAgICAgaW5pdGlhbFJlbWFpbmluZ1RpbWU9e3RpbWVTdGFydH1cbiAgICAgICAgICAgIHN0cm9rZVdpZHRoPXs1fVxuICAgICAgICAgICAgdHJhaWxDb2xvcj17aXNQbGF5aW5nID8gXCIjQzBEM0RCXCIgOiBcIiNjY2NjY2NcIn1cbiAgICAgICAgICAgIHN0cm9rZUxpbmVjYXA9XCJzcXVhcmVcIlxuICAgICAgICAgICAgb25Db21wbGV0ZT17aGFuZGxlQ29tcGxldGV9XG4gICAgICAgICAgPlxuXG5cbiAgICAgICAgICAgIHsoeyByZW1haW5pbmdUaW1lIH0pID0+IHtcblxuICAgICAgICAgICAgICBpZiAoeHggIT09IHJlbWFpbmluZ1RpbWUpIHtcbiAgICAgICAgICAgICAgICBzZXRUaW1lb3V0KCgpID0+IHNldFh4KHJlbWFpbmluZ1RpbWUpLCAwKTsgLy8gYXZvaWQgc2V0dGluZyBzdGF0ZSBkdXJpbmcgcmVuZGVyXG4gICAgICAgICAgICAgICAgY29uc3QgciA9IHBhcnNlRmxvYXQocmVtYWluaW5nVGltZSk7XG4gICAgICAgICAgICAgICAgY29uc3QgZCA9IHBhcnNlRmxvYXQodGltZSk7XG4gICAgICAgICAgICAgICAgY3VycmVudFZhbHVlLnNldFRleHRWYWx1ZShcIlwiICsgKChkLXIpLzg2NDAwKS50b0ZpeGVkKDgpKTtcbiAgICAgICAgICAgICAgfVxuXG4gICAgICAgICAgICAgIHJldHVybiAoXG4gICAgICAgICAgICAgICAgPHNwYW4+XG4gICAgICAgICAgICAgICAgICA8YnV0dG9uIG9uQ2xpY2s9e29uUGF1c2VDbGlja0hhbmRsZXJ9IHN0eWxlPXt7IGJvcmRlcjogJ25vbmUnLCBiYWNrZ3JvdW5kOiAndHJhbnNwYXJlbnQnLCBjdXJzb3I6ICdwb2ludGVyJyB9fSBjbGFzc05hbWU9XCJ0aW1lci1idXR0b25cIj5cbiAgICAgICAgICAgICAgICAgICAge2lzUGxheWluZyA/IDxQYXVzZUljb24gY2xhc3NOYW1lPVwidGltZXItaWNvbi1idXR0b25cIiAvPiA6IDxTdGFydEljb24gY2xhc3NOYW1lPVwidGltZXItaWNvbi1idXR0b25cIiAvPn1cbiAgICAgICAgICAgICAgICAgIDwvYnV0dG9uPlxuICAgICAgICAgICAgICAgICAgPHNwYW4+XG4gICAgICAgICAgICAgICAgICAgIHtyZW5kZXJUaW1lKFwic2Vjb25kc1wiLCByZW1haW5pbmdUaW1lLCB0aW1lLCBhbGxvY2F0ZWRUaW1lLCBmYWxzZSl9XG4gICAgICAgICAgICAgICAgICA8L3NwYW4+XG4gICAgICAgICAgICAgICAgPC9zcGFuPlxuICAgICAgICAgICAgICApO1xuICAgICAgICAgICAgfX1cblxuICAgICAgICAgIDwvQ291bnRkb3duQ2lyY2xlVGltZXI+XG4gICAgICAgIDwvZGl2PlxuICAgICAgKVxuICAgIH1cblxuICB9XG59XG4iLCJpbXBvcnQgeyBjcmVhdGVFbGVtZW50LCB1c2VTdGF0ZSwgdXNlRWZmZWN0LCBtZW1vIH0gZnJvbSBcInJlYWN0XCI7XG5cbmltcG9ydCB7IENvdW50ZG93blRpbWVyQ29tcG9uZW50IH0gZnJvbSBcIi4vY29tcG9uZW50cy9Db3VudGRvd25UaW1lckNvbXBvbmVudFwiO1xuaW1wb3J0IFwiLi91aS9Db3VudGRvd25UaW1lci5jc3NcIjtcblxuLy8gSGVscGVyIGZ1bmN0aW9uIHRvIGNvbXBhcmUgTWVuZGl4LWxpa2Ugb2JqZWN0cyAoc3RhdHVzIGFuZCB2YWx1ZSlcbmNvbnN0IGFyZU1lbmRpeFByb3BzRXF1YWwgPSAocHJldiwgbmV4dCkgPT4ge1xuICBpZiAoIXByZXYgfHwgIW5leHQpIHJldHVybiBwcmV2ID09PSBuZXh0O1xuICBpZiAocHJldi5zdGF0dXMgIT09IG5leHQuc3RhdHVzKSByZXR1cm4gZmFsc2U7XG5cbiAgaWYgKHByZXYudmFsdWUgaW5zdGFuY2VvZiBEYXRlICYmIG5leHQudmFsdWUgaW5zdGFuY2VvZiBEYXRlKSB7XG4gICAgcmV0dXJuIHByZXYudmFsdWUuZ2V0VGltZSgpID09PSBuZXh0LnZhbHVlLmdldFRpbWUoKTtcbiAgfVxuICAvLyBGb3IgbnVtYmVycywgc3RyaW5ncywgZXRjLlxuICByZXR1cm4gcHJldi52YWx1ZSA9PT0gbmV4dC52YWx1ZTtcbn07XG5cbi8vIEN1c3RvbSBjb21wYXJpc29uIGZ1bmN0aW9uIGZvciBSZWFjdC5tZW1vXG5jb25zdCBhcmVDb3VudGRvd25Qcm9wc0VxdWFsID0gKHByZXZQcm9wcywgbmV4dFByb3BzKSA9PiB7XG4gIC8vIENvbXBhcmUgZWFjaCBNZW5kaXgtbGlrZSBwcm9wIHVzaW5nIHlvdXIgaGVscGVyXG4gIGNvbnN0IGN1cnJlbnREYXRlVGltZUNoYW5nZWQgPSAhYXJlTWVuZGl4UHJvcHNFcXVhbChwcmV2UHJvcHMuY3VycmVudERhdGVUaW1lLCBuZXh0UHJvcHMuY3VycmVudERhdGVUaW1lKTtcbiAgLy8gSWYgdHJ1ZSwgaXQgbWVhbnMgYSBwcm9wIGhhcyBjaGFuZ2VkLCBzbyByZXR1cm4gZmFsc2UgKG1lYW5pbmcgcmUtcmVuZGVyKVxuICBpZiAoY3VycmVudERhdGVUaW1lQ2hhbmdlZCkge1xuICAgIHJldHVybiBmYWxzZTtcbiAgfVxuICAvLyBJZiBubyByZWxldmFudCBwcm9wIGhhcyBjaGFuZ2VkLCByZXR1cm4gdHJ1ZSAobWVhbmluZyBETyBOT1QgcmUtcmVuZGVyKVxuICByZXR1cm4gdHJ1ZTtcbn07XG5cblxuZXhwb3J0IGNvbnN0IENvdW50ZG93blRpbWVyID0gbWVtbyhmdW5jdGlvbiBDb3VudGRvd25UaW1lcih7XG4gIHRpbWVEdXJhdGlvbixcbiAgY3VycmVudFZhbHVlLFxuICBzdGFydFRpbWUsXG4gIG9uUGF1c2VDbGlja0FjdGlvbixcbiAgY3VycmVudERhdGVUaW1lXG59KSB7XG4gIGNvbnN0IFt0aW1lLCBzZXRUaW1lXSA9IHVzZVN0YXRlKHRpbWVEdXJhdGlvbik7XG4gIGNvbnN0IFtjLCBzZXRDXSA9IHVzZVN0YXRlKGN1cnJlbnRWYWx1ZSk7XG4gIGNvbnN0IFt0aW1lU3RhcnQsIHNldFRpbWVTdGFydF0gPSB1c2VTdGF0ZShzdGFydFRpbWUpO1xuICBjb25zdCBbZGF0ZSwgc2V0RGF0ZV0gPSB1c2VTdGF0ZShjdXJyZW50RGF0ZVRpbWUpO1xuXG4gIHVzZUVmZmVjdCgoKSA9PiB7XG4gICAgLy8gVGhpcyB3aWxsIG9ubHkgcnVuIGlmIHRpbWVEdXJhdGlvbiAocHJvcCkgY2hhbmdlcyBBTkQgUmVhY3QubWVtbyBhbGxvd2VkIGEgcmUtcmVuZGVyXG4gICAgc2V0VGltZSh0aW1lRHVyYXRpb24pO1xuICB9LCBbdGltZUR1cmF0aW9uXSk7XG5cbiAgdXNlRWZmZWN0KCgpID0+IHtcbiAgICBzZXRUaW1lU3RhcnQoc3RhcnRUaW1lKTtcbiAgfSwgW3N0YXJ0VGltZV0pO1xuXG4gIHVzZUVmZmVjdCgoKSA9PiB7XG4gICAgc2V0QyhjdXJyZW50VmFsdWUpO1xuICB9LCBbY3VycmVudFZhbHVlXSk7XG5cbiAgdXNlRWZmZWN0KCgpID0+IHtcbiAgICBzZXREYXRlKGN1cnJlbnREYXRlVGltZSk7XG4gIH0sIFtjdXJyZW50RGF0ZVRpbWVdKTtcblxuICBpZiAodGltZS5zdGF0dXMgIT09IFwiYXZhaWxhYmxlXCIgfHwgYy5zdGF0dXMgIT09IFwiYXZhaWxhYmxlXCIgfHwgdGltZVN0YXJ0LnN0YXR1cyAhPT0gXCJhdmFpbGFibGVcIiB8fCBkYXRlLnN0YXR1cyAhPT0gXCJhdmFpbGFibGVcIikge1xuICAgIHJldHVybiA8ZGl2PkxvYWRpbmcuLi48L2Rpdj47XG4gIH0gZWxzZSB7XG4gICAgcmV0dXJuIChcbiAgICAgIDxDb3VudGRvd25UaW1lckNvbXBvbmVudFxuICAgICAgICB0aW1lRHVyYXRpb249e3RpbWUudmFsdWUudG9OdW1iZXIoKX1cbiAgICAgICAgY3VycmVudFZhbHVlPXtjfVxuICAgICAgICBzdGFydFRpbWU9e3RpbWVTdGFydC52YWx1ZS50b051bWJlcigpfVxuICAgICAgICBvblBhdXNlQ2xpY2tBY3Rpb249e29uUGF1c2VDbGlja0FjdGlvbn1cbiAgICAgICAgY3VycmVudERhdGVUaW1lPXtkYXRlLnZhbHVlfVxuICAgICAgLz5cbiAgICApO1xuICB9XG59LCBhcmVDb3VudGRvd25Qcm9wc0VxdWFsKTsgLy8gPC0tIFBhc3MgdGhlIGN1c3RvbSBjb21wYXJpc29uIGZ1bmN0aW9uIGhlcmUiXSwibmFtZXMiOlsiRyIsIndpbmRvdyIsIk0iLCJMIiwiSSIsImlzUGxheWluZyIsIm8iLCJkdXJhdGlvbiIsImUiLCJzdGFydEF0IiwibiIsInVwZGF0ZUludGVydmFsIiwidCIsIm9uQ29tcGxldGUiLCJzIiwib25VcGRhdGUiLCJyIiwiaSIsImMiLCJFIiwibSIsImIiLCJwIiwiZiIsInUiLCJhIiwiaCIsInciLCJnIiwibCIsImN1cnJlbnQiLCJyZXF1ZXN0QW5pbWF0aW9uRnJhbWUiLCJkIiwiQyIsImsiLCJSIiwidiIsIiQiLCJjYW5jZWxBbmltYXRpb25GcmFtZSIsImNsZWFyVGltZW91dCIsInkiLCJxIiwic2hvdWxkUmVwZWF0IiwiZGVsYXkiLCJuZXdTdGFydEF0Iiwic2V0VGltZW91dCIsImVsYXBzZWRUaW1lIiwicmVzZXQiLCJBIiwiTWF0aCIsIlBJIiwicGF0aCIsInBhdGhMZW5ndGgiLCJUIiwiQiIsInBvc2l0aW9uIiwid2lkdGgiLCJoZWlnaHQiLCJQIiwiZGlzcGxheSIsImp1c3RpZnlDb250ZW50IiwiYWxpZ25JdGVtcyIsImxlZnQiLCJ0b3AiLCJGIiwiVyIsInJlcGxhY2UiLCJzdWJzdHJpbmciLCJtYXRjaCIsIm1hcCIsInBhcnNlSW50IiwiaiIsImNvbG9ycyIsImNvbG9yc1RpbWUiLCJpc1Ntb290aENvbG9yVHJhbnNpdGlvbiIsImZpbmRJbmRleCIsImlzR3Jvd2luZyIsImpvaW4iLCJTIiwiaW5pdGlhbFJlbWFpbmluZ1RpbWUiLCJzaXplIiwic3Ryb2tlV2lkdGgiLCJ0cmFpbFN0cm9rZVdpZHRoIiwicm90YXRpb24iLCJVIiwibWF4IiwiY2VpbCIsIm5ld0luaXRpYWxSZW1haW5pbmdUaW1lIiwicmVtYWluaW5nVGltZSIsInN0cm9rZSIsInN0cm9rZURhc2hvZmZzZXQiLCJEIiwiY2hpbGRyZW4iLCJzdHJva2VMaW5lY2FwIiwidHJhaWxDb2xvciIsIngiLCJjcmVhdGVFbGVtZW50Iiwic3R5bGUiLCJ2aWV3Qm94IiwieG1sbnMiLCJmaWxsIiwic3Ryb2tlRGFzaGFycmF5IiwiY29sb3IiLCJkaXNwbGF5TmFtZSIsInJlbmRlclRpbWUiLCJkaW1lbnNpb24iLCJ0aW1lIiwidGltZUR1cmF0aW9uIiwiYWxsb2NhdGVkVGltZSIsImlzT3ZlcnNob290Iiwicm91bmQiLCJtaW51dGVzIiwiZmxvb3IiLCJ0b1N0cmluZyIsInBhZFN0YXJ0Iiwic2Vjb25kcyIsInRydW5jIiwiZm9ybWF0ZWRIb3VycyIsImZvcm1hdGVkbWludXRlcyIsImNsYXNzTmFtZSIsImJvdHRvbSIsIkNvdW50ZG93blRpbWVyQ29tcG9uZW50IiwiY3VycmVudFZhbHVlIiwic3RhcnRUaW1lIiwib25QYXVzZUNsaWNrQWN0aW9uIiwiY3VycmVudERhdGVUaW1lIiwic2V0SXNQbGF5aW5nIiwidXNlU3RhdGUiLCJzZXRUaW1lIiwidGltZVN0YXJ0Iiwic2V0VGltZVN0YXJ0Iiwia2V5Iiwic2V0S2V5IiwieHgiLCJzZXRYeCIsImRhdGUiLCJzZXREYXRlIiwiZGVmYXVsdFRpbWUiLCJzZXREZWZhdWx0VGltZSIsIk51bWJlciIsIk1BWF9TQUZFX0lOVEVHRVIiLCJzZXRBbGxvY2F0ZWRUaW1lIiwiY291bnRVcE1vZGUiLCJzZXRDb3VudFVwTW9kZSIsImlzTG9hZGluZyIsInNldElzTG9hZGluZyIsInVzZUVmZmVjdCIsImhhbmRsZUNvbXBsZXRlIiwiUGF1c2VJY29uIiwiU3RhcnRJY29uIiwib25QYXVzZUNsaWNrSGFuZGxlciIsImNhbkV4ZWN1dGUiLCJleGVjdXRlIiwicHJldiIsIkNvdW50ZG93bkNpcmNsZVRpbWVyIiwic3RhdHVzIiwicGFyc2VGbG9hdCIsInNldFRleHRWYWx1ZSIsInRvRml4ZWQiLCJvbkNsaWNrIiwiYm9yZGVyIiwiYmFja2dyb3VuZCIsImN1cnNvciIsImFyZU1lbmRpeFByb3BzRXF1YWwiLCJuZXh0IiwidmFsdWUiLCJEYXRlIiwiZ2V0VGltZSIsImFyZUNvdW50ZG93blByb3BzRXF1YWwiLCJwcmV2UHJvcHMiLCJuZXh0UHJvcHMiLCJjdXJyZW50RGF0ZVRpbWVDaGFuZ2VkIiwiQ291bnRkb3duVGltZXIiLCJtZW1vIiwic2V0QyIsInRvTnVtYmVyIl0sIm1hcHBp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
