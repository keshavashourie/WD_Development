import x, { useRef, useState, useCallback, useEffect, useLayoutEffect, createElement, memo } from 'react';

var G = typeof window == "undefined" ? useEffect : useLayoutEffect,
  I = ({
    isPlaying: o,
    duration: e,
    startAt: n = 0,
    updateInterval: t = 0,
    onComplete: s,
    onUpdate: r
  }) => {
    let [i, c] = useState(n),
      m = useRef(0),
      p = useRef(n),
      f = useRef(n * -1e3),
      u = useRef(null),
      a = useRef(null),
      h = useRef(null),
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
      y = useCallback(g => {
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
      a = useRef(),
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
  return createElement("div", {
    className: "time-wrapper"
  }, createElement("div", {
    className: "time",
    style: {
      bottom: '90%'
    }
  }, createElement("div", {
    className: "task-name"
  }, "Task Time"), isOvershoot ? createElement("div", {
    className: "time-count time-count-overshoot"
  }, "+ ", minutes, ":", seconds, " min") : createElement("div", {
    className: "time-count"
  }, minutes, ":", seconds, " min"), timeDuration != 0 && allocatedTime != 0 || timeDuration == 0 && allocatedTime != 0 ? createElement("div", {
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
  const [isPlaying, setIsPlaying] = useState(true);
  const [time, setTime] = useState(timeDuration);
  const [timeStart, setTimeStart] = useState(startTime);
  const [key, setKey] = useState(0);
  const [xx, setXx] = useState(timeDuration);
  const [date, setDate] = useState(currentDateTime);
  const [defaultTime, setDefaultTime] = useState(Number.MAX_SAFE_INTEGER);
  const [allocatedTime, setAllocatedTime] = useState(timeDuration);
  const [countUpMode, setCountUpMode] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  useEffect(() => {
    setTime(timeDuration);
    setAllocatedTime(timeDuration);
  }, [timeDuration]);
  useEffect(() => {
    setTimeStart(startTime);
  }, [startTime]);
  useEffect(() => {
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
  }) => createElement("svg", {
    xmlns: "http://www.w3.org/2000/svg",
    viewBox: "0 0 24 24",
    className: className
  }, createElement("path", {
    fill: "none",
    d: "M0 0h24v24H0z"
  }), createElement("path", {
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
  }) => createElement("svg", {
    xmlns: "http://www.w3.org/2000/svg",
    viewBox: "0 0 24 24",
    className: className
  }, createElement("path", {
    fill: "none",
    d: "M0 0h24v24H0z"
  }), createElement("path", {
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
    return createElement("div", {
      className: "timer-container"
    }, createElement(D, {
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
      return createElement("span", null, createElement("button", {
        onClick: onPauseClickHandler,
        style: {
          border: 'none',
          background: 'transparent',
          cursor: 'pointer'
        },
        className: "timer-button"
      }, isPlaying ? createElement(PauseIcon, {
        className: "timer-icon-button"
      }) : createElement(StartIcon, {
        className: "timer-icon-button"
      })), createElement("span", null, renderTime("seconds", elapsedTime, 0, allocatedTime, false)));
    }));
  } else {
    if (countUpMode) {
      // When AllocationTime > 0 && In case of overshoot
      return createElement("div", {
        className: "timer-container"
      }, createElement(D, {
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
        return createElement("span", null, createElement("button", {
          onClick: onPauseClickHandler,
          style: {
            border: 'none',
            background: 'transparent',
            cursor: 'pointer'
          },
          className: "timer-button"
        }, isPlaying ? createElement(PauseIcon, {
          className: "timer-icon-button"
        }) : createElement(StartIcon, {
          className: "timer-icon-button"
        })), createElement("span", null, renderTime("seconds", elapsedTime > time ? elapsedTime - time : elapsedTime, 0, allocatedTime, true)));
      }));
    } else {
      // When AllocationTime > 0
      return createElement("div", {
        className: "timer-container"
      }, createElement(D, {
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
        return createElement("span", null, createElement("button", {
          onClick: onPauseClickHandler,
          style: {
            border: 'none',
            background: 'transparent',
            cursor: 'pointer'
          },
          className: "timer-button"
        }, isPlaying ? createElement(PauseIcon, {
          className: "timer-icon-button"
        }) : createElement(StartIcon, {
          className: "timer-icon-button"
        })), createElement("span", null, renderTime("seconds", remainingTime, time, allocatedTime, false)));
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
const CountdownTimer = memo(function CountdownTimer({
  timeDuration,
  currentValue,
  startTime,
  onPauseClickAction,
  currentDateTime
}) {
  const [time, setTime] = useState(timeDuration);
  const [c, setC] = useState(currentValue);
  const [timeStart, setTimeStart] = useState(startTime);
  const [date, setDate] = useState(currentDateTime);
  useEffect(() => {
    // This will only run if timeDuration (prop) changes AND React.memo allowed a re-render
    setTime(timeDuration);
  }, [timeDuration]);
  useEffect(() => {
    setTimeStart(startTime);
  }, [startTime]);
  useEffect(() => {
    setC(currentValue);
  }, [currentValue]);
  useEffect(() => {
    setDate(currentDateTime);
  }, [currentDateTime]);
  if (time.status !== "available" || c.status !== "available" || timeStart.status !== "available" || date.status !== "available") {
    return createElement("div", null, "Loading...");
  } else {
    return createElement(CountdownTimerComponent, {
      timeDuration: time.value.toNumber(),
      currentValue: c,
      startTime: timeStart.value.toNumber(),
      onPauseClickAction: onPauseClickAction,
      currentDateTime: date.value
    });
  }
}, areCountdownPropsEqual); // <-- Pass the custom comparison function here

export { CountdownTimer };
//# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiQ291bnRkb3duVGltZXIubWpzIiwic291cmNlcyI6WyIuLi8uLi8uLi8uLi8uLi9ub2RlX21vZHVsZXMvcmVhY3QtY291bnRkb3duLWNpcmNsZS10aW1lci9saWIvaW5kZXgubW9kdWxlLmpzIiwiLi4vLi4vLi4vLi4vLi4vc3JjL2NvbXBvbmVudHMvQ291bnRkb3duVGltZXJDb21wb25lbnQuanN4IiwiLi4vLi4vLi4vLi4vLi4vc3JjL0NvdW50ZG93blRpbWVyLmpzeCJdLCJzb3VyY2VzQ29udGVudCI6WyJpbXBvcnQgeCBmcm9tXCJyZWFjdFwiO2ltcG9ydHt1c2VSZWYgYXMgVX1mcm9tXCJyZWFjdFwiO2ltcG9ydHt1c2VTdGF0ZSBhcyBFLHVzZVJlZiBhcyBiLHVzZUNhbGxiYWNrIGFzIHF9ZnJvbVwicmVhY3RcIjtpbXBvcnR7dXNlRWZmZWN0IGFzIE0sdXNlTGF5b3V0RWZmZWN0IGFzIEx9ZnJvbVwicmVhY3RcIjt2YXIgRz10eXBlb2Ygd2luZG93PT1cInVuZGVmaW5lZFwiP006TCxJPSh7aXNQbGF5aW5nOm8sZHVyYXRpb246ZSxzdGFydEF0Om49MCx1cGRhdGVJbnRlcnZhbDp0PTAsb25Db21wbGV0ZTpzLG9uVXBkYXRlOnJ9KT0+e2xldFtpLGNdPUUobiksbT1iKDApLHA9YihuKSxmPWIobiotMWUzKSx1PWIobnVsbCksYT1iKG51bGwpLGg9YihudWxsKSx3PWc9PntsZXQgbD1nLzFlMztpZihhLmN1cnJlbnQ9PT1udWxsKXthLmN1cnJlbnQ9bCx1LmN1cnJlbnQ9cmVxdWVzdEFuaW1hdGlvbkZyYW1lKHcpO3JldHVybn1sZXQgZD1sLWEuY3VycmVudCxDPW0uY3VycmVudCtkO2EuY3VycmVudD1sLG0uY3VycmVudD1DO2xldCBrPXAuY3VycmVudCsodD09PTA/QzooQy90fDApKnQpLFI9cC5jdXJyZW50K0Msdj10eXBlb2YgZT09XCJudW1iZXJcIiYmUj49ZTtjKHY/ZTprKSx2fHwodS5jdXJyZW50PXJlcXVlc3RBbmltYXRpb25GcmFtZSh3KSl9LCQ9KCk9Pnt1LmN1cnJlbnQmJmNhbmNlbEFuaW1hdGlvbkZyYW1lKHUuY3VycmVudCksaC5jdXJyZW50JiZjbGVhclRpbWVvdXQoaC5jdXJyZW50KSxhLmN1cnJlbnQ9bnVsbH0seT1xKGc9PnskKCksbS5jdXJyZW50PTA7bGV0IGw9dHlwZW9mIGc9PVwibnVtYmVyXCI/ZzpuO3AuY3VycmVudD1sLGMobCksbyYmKHUuY3VycmVudD1yZXF1ZXN0QW5pbWF0aW9uRnJhbWUodykpfSxbbyxuXSk7cmV0dXJuIEcoKCk9PntpZihyPT1udWxsfHxyKGkpLGUmJmk+PWUpe2YuY3VycmVudCs9ZSoxZTM7bGV0e3Nob3VsZFJlcGVhdDpnPSExLGRlbGF5Omw9MCxuZXdTdGFydEF0OmR9PShzPT1udWxsP3ZvaWQgMDpzKGYuY3VycmVudC8xZTMpKXx8e307ZyYmKGguY3VycmVudD1zZXRUaW1lb3V0KCgpPT55KGQpLGwqMWUzKSl9fSxbaSxlXSksRygoKT0+KG8mJih1LmN1cnJlbnQ9cmVxdWVzdEFuaW1hdGlvbkZyYW1lKHcpKSwkKSxbbyxlLHRdKSx7ZWxhcHNlZFRpbWU6aSxyZXNldDp5fX07dmFyIEE9KG8sZSxuKT0+e2xldCB0PW8vMixzPWUvMixyPXQtcyxpPTIqcixjPW49PT1cImNsb2Nrd2lzZVwiP1wiMSwwXCI6XCIwLDFcIixtPTIqTWF0aC5QSSpyO3JldHVybntwYXRoOmBtICR7dH0sJHtzfSBhICR7cn0sJHtyfSAwICR7Y30gMCwke2l9IGEgJHtyfSwke3J9IDAgJHtjfSAwLC0ke2l9YCxwYXRoTGVuZ3RoOm19fSxUPShvLGUpPT5vPT09MHx8bz09PWU/MDp0eXBlb2YgZT09XCJudW1iZXJcIj9vLWU6MCxCPW89Pih7cG9zaXRpb246XCJyZWxhdGl2ZVwiLHdpZHRoOm8saGVpZ2h0Om99KSxQPXtkaXNwbGF5OlwiZmxleFwiLGp1c3RpZnlDb250ZW50OlwiY2VudGVyXCIsYWxpZ25JdGVtczpcImNlbnRlclwiLHBvc2l0aW9uOlwiYWJzb2x1dGVcIixsZWZ0OjAsdG9wOjAsd2lkdGg6XCIxMDAlXCIsaGVpZ2h0OlwiMTAwJVwifTt2YXIgRj0obyxlLG4sdCxzKT0+e2lmKHQ9PT0wKXJldHVybiBlO2xldCByPShzP3QtbzpvKS90O3JldHVybiBlK24qcn0sVz1vPT57dmFyIGUsbjtyZXR1cm4obj0oZT1vLnJlcGxhY2UoL14jPyhbYS1mXFxkXSkoW2EtZlxcZF0pKFthLWZcXGRdKSQvaSwodCxzLHIsaSk9PmAjJHtzfSR7c30ke3J9JHtyfSR7aX0ke2l9YCkuc3Vic3RyaW5nKDEpLm1hdGNoKC8uezJ9L2cpKT09bnVsbD92b2lkIDA6ZS5tYXAodD0+cGFyc2VJbnQodCwxNikpKSE9bnVsbD9uOltdfSxqPShvLGUpPT57dmFyIHU7bGV0e2NvbG9yczpuLGNvbG9yc1RpbWU6dCxpc1Ntb290aENvbG9yVHJhbnNpdGlvbjpzPSEwfT1vO2lmKHR5cGVvZiBuPT1cInN0cmluZ1wiKXJldHVybiBuO2xldCByPSh1PXQ9PW51bGw/dm9pZCAwOnQuZmluZEluZGV4KChhLGgpPT5hPj1lJiZlPj10W2grMV0pKSE9bnVsbD91Oi0xO2lmKCF0fHxyPT09LTEpcmV0dXJuIG5bMF07aWYoIXMpcmV0dXJuIG5bcl07bGV0IGk9dFtyXS1lLGM9dFtyXS10W3IrMV0sbT1XKG5bcl0pLHA9VyhuW3IrMV0pLGY9ISFvLmlzR3Jvd2luZztyZXR1cm5gcmdiKCR7bS5tYXAoKGEsaCk9PkYoaSxhLHBbaF0tYSxjLGYpfDApLmpvaW4oXCIsXCIpfSlgfSxTPW89PntsZXR7ZHVyYXRpb246ZSxpbml0aWFsUmVtYWluaW5nVGltZTpuLHVwZGF0ZUludGVydmFsOnQsc2l6ZTpzPTE4MCxzdHJva2VXaWR0aDpyPTEyLHRyYWlsU3Ryb2tlV2lkdGg6aSxpc1BsYXlpbmc6Yz0hMSxpc0dyb3dpbmc6bT0hMSxyb3RhdGlvbjpwPVwiY2xvY2t3aXNlXCIsb25Db21wbGV0ZTpmLG9uVXBkYXRlOnV9PW8sYT1VKCksaD1NYXRoLm1heChyLGkhPW51bGw/aTowKSx7cGF0aDp3LHBhdGhMZW5ndGg6JH09QShzLGgscCkse2VsYXBzZWRUaW1lOnl9PUkoe2lzUGxheWluZzpjLGR1cmF0aW9uOmUsc3RhcnRBdDpUKGUsbiksdXBkYXRlSW50ZXJ2YWw6dCxvblVwZGF0ZTp0eXBlb2YgdT09XCJmdW5jdGlvblwiP2w9PntsZXQgZD1NYXRoLmNlaWwoZS1sKTtkIT09YS5jdXJyZW50JiYoYS5jdXJyZW50PWQsdShkKSl9OnZvaWQgMCxvbkNvbXBsZXRlOnR5cGVvZiBmPT1cImZ1bmN0aW9uXCI/bD0+e3ZhciBSO2xldHtzaG91bGRSZXBlYXQ6ZCxkZWxheTpDLG5ld0luaXRpYWxSZW1haW5pbmdUaW1lOmt9PShSPWYobCkpIT1udWxsP1I6e307aWYoZClyZXR1cm57c2hvdWxkUmVwZWF0OmQsZGVsYXk6QyxuZXdTdGFydEF0OlQoZSxrKX19OnZvaWQgMH0pLGc9ZS15O3JldHVybntlbGFwc2VkVGltZTp5LHBhdGg6dyxwYXRoTGVuZ3RoOiQscmVtYWluaW5nVGltZTpNYXRoLmNlaWwoZykscm90YXRpb246cCxzaXplOnMsc3Ryb2tlOmoobyxnKSxzdHJva2VEYXNob2Zmc2V0OkYoeSwwLCQsZSxtKSxzdHJva2VXaWR0aDpyfX07dmFyIEQ9bz0+e2xldHtjaGlsZHJlbjplLHN0cm9rZUxpbmVjYXA6bix0cmFpbENvbG9yOnQsdHJhaWxTdHJva2VXaWR0aDpzfT1vLHtwYXRoOnIscGF0aExlbmd0aDppLHN0cm9rZTpjLHN0cm9rZURhc2hvZmZzZXQ6bSxyZW1haW5pbmdUaW1lOnAsZWxhcHNlZFRpbWU6ZixzaXplOnUsc3Ryb2tlV2lkdGg6YX09UyhvKTtyZXR1cm4geC5jcmVhdGVFbGVtZW50KFwiZGl2XCIse3N0eWxlOkIodSl9LHguY3JlYXRlRWxlbWVudChcInN2Z1wiLHt2aWV3Qm94OmAwIDAgJHt1fSAke3V9YCx3aWR0aDp1LGhlaWdodDp1LHhtbG5zOlwiaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmdcIn0seC5jcmVhdGVFbGVtZW50KFwicGF0aFwiLHtkOnIsZmlsbDpcIm5vbmVcIixzdHJva2U6dCE9bnVsbD90OlwiI2Q5ZDlkOVwiLHN0cm9rZVdpZHRoOnMhPW51bGw/czphfSkseC5jcmVhdGVFbGVtZW50KFwicGF0aFwiLHtkOnIsZmlsbDpcIm5vbmVcIixzdHJva2U6YyxzdHJva2VMaW5lY2FwOm4hPW51bGw/bjpcInJvdW5kXCIsc3Ryb2tlV2lkdGg6YSxzdHJva2VEYXNoYXJyYXk6aSxzdHJva2VEYXNob2Zmc2V0Om19KSksdHlwZW9mIGU9PVwiZnVuY3Rpb25cIiYmeC5jcmVhdGVFbGVtZW50KFwiZGl2XCIse3N0eWxlOlB9LGUoe3JlbWFpbmluZ1RpbWU6cCxlbGFwc2VkVGltZTpmLGNvbG9yOmN9KSkpfTtELmRpc3BsYXlOYW1lPVwiQ291bnRkb3duQ2lyY2xlVGltZXJcIjtleHBvcnR7RCBhcyBDb3VudGRvd25DaXJjbGVUaW1lcixTIGFzIHVzZUNvdW50ZG93bn07XG4iLCJpbXBvcnQgeyBjcmVhdGVFbGVtZW50LCB1c2VTdGF0ZSwgdXNlRWZmZWN0IH0gZnJvbSBcInJlYWN0XCI7XG5pbXBvcnQgeyBDb3VudGRvd25DaXJjbGVUaW1lciB9IGZyb20gJ3JlYWN0LWNvdW50ZG93bi1jaXJjbGUtdGltZXInXG5cbmNvbnN0IHJlbmRlclRpbWUgPSAoZGltZW5zaW9uLCB0aW1lLCB0aW1lRHVyYXRpb24sIGFsbG9jYXRlZFRpbWUsIGlzT3ZlcnNob290KSA9PiB7XG4gIGNvbnN0IGR1cmF0aW9uID0gTWF0aC5yb3VuZChhbGxvY2F0ZWRUaW1lKTtcbiBcbiAgY29uc3QgbWludXRlcyA9IE1hdGguZmxvb3IodGltZSAvIDYwKS50b1N0cmluZygpLnBhZFN0YXJ0KDIsICcwJyk7XG4gIGNvbnN0IHNlY29uZHMgPSBNYXRoLnRydW5jKCh0aW1lICUgNjApKS50b1N0cmluZygpLnBhZFN0YXJ0KDIsICcwJyk7O1xuIFxuICAvL09ubHkgdXNlZCBmb3IgZGlzcGxheS5cbiAgY29uc3QgZm9ybWF0ZWRIb3VycyA9IE1hdGguZmxvb3IoZHVyYXRpb24gLyAzNjAwKS50b1N0cmluZygpLnBhZFN0YXJ0KDIsICcwJyk7XG4gIGNvbnN0IGZvcm1hdGVkbWludXRlcyA9IE1hdGguZmxvb3IoKGR1cmF0aW9uICUgMzYwMCkgLyA2MCkudG9TdHJpbmcoKS5wYWRTdGFydCgyLCAnMCcpO1xuXG4gXG4gIHJldHVybiAoXG4gICAgICA8ZGl2IGNsYXNzTmFtZT1cInRpbWUtd3JhcHBlclwiPlxuICAgICAgPGRpdiBjbGFzc05hbWU9XCJ0aW1lXCIgc3R5bGU9e3sgYm90dG9tOiAnOTAlJyB9fT5cbiAgICAgICAgPGRpdiBjbGFzc05hbWU9XCJ0YXNrLW5hbWVcIj5UYXNrIFRpbWU8L2Rpdj5cbiAgICAgICAge2lzT3ZlcnNob290Pyg8ZGl2IGNsYXNzTmFtZT1cInRpbWUtY291bnQgdGltZS1jb3VudC1vdmVyc2hvb3RcIj4rIHttaW51dGVzfTp7c2Vjb25kc30gbWluPC9kaXY+KTooPGRpdiBjbGFzc05hbWU9XCJ0aW1lLWNvdW50XCI+e21pbnV0ZXN9OntzZWNvbmRzfSBtaW48L2Rpdj4pfVxuICAgICAgICB7KCh0aW1lRHVyYXRpb24gIT0gMCAmJiBhbGxvY2F0ZWRUaW1lICE9IDApIHx8ICh0aW1lRHVyYXRpb24gPT0gMCAmJiBhbGxvY2F0ZWRUaW1lICE9IDApKSA/ICg8ZGl2IGNsYXNzTmFtZT1cInRhc2stbmFtZVwiPi97Zm9ybWF0ZWRIb3Vyc306e2Zvcm1hdGVkbWludXRlc30gaDwvZGl2PikgOiBudWxsfVxuICAgICAgPC9kaXY+XG4gICAgPC9kaXY+XG4gICk7XG59O1xuXG5cblxuZXhwb3J0IGZ1bmN0aW9uIENvdW50ZG93blRpbWVyQ29tcG9uZW50KHsgdGltZUR1cmF0aW9uLCBjdXJyZW50VmFsdWUsIHN0YXJ0VGltZSwgb25QYXVzZUNsaWNrQWN0aW9uLCBjdXJyZW50RGF0ZVRpbWUgfSkge1xuXG4gIGNvbnN0IFtpc1BsYXlpbmcsIHNldElzUGxheWluZ10gPSB1c2VTdGF0ZSh0cnVlKTtcbiAgY29uc3QgW3RpbWUsIHNldFRpbWVdID0gdXNlU3RhdGUodGltZUR1cmF0aW9uKTtcbiAgY29uc3QgW3RpbWVTdGFydCwgc2V0VGltZVN0YXJ0XSA9IHVzZVN0YXRlKHN0YXJ0VGltZSk7XG4gIGNvbnN0IFtrZXksIHNldEtleV0gPSB1c2VTdGF0ZSgwKTtcbiAgY29uc3QgW3h4LCBzZXRYeF0gPSB1c2VTdGF0ZSh0aW1lRHVyYXRpb24pO1xuICBjb25zdCBbZGF0ZSwgc2V0RGF0ZV0gPSB1c2VTdGF0ZShjdXJyZW50RGF0ZVRpbWUpO1xuICBjb25zdCBbZGVmYXVsdFRpbWUsIHNldERlZmF1bHRUaW1lXSA9IHVzZVN0YXRlKE51bWJlci5NQVhfU0FGRV9JTlRFR0VSKTtcbiAgY29uc3QgW2FsbG9jYXRlZFRpbWUsIHNldEFsbG9jYXRlZFRpbWVdID0gdXNlU3RhdGUodGltZUR1cmF0aW9uKTtcbiAgY29uc3QgW2NvdW50VXBNb2RlLCBzZXRDb3VudFVwTW9kZV0gPSB1c2VTdGF0ZShmYWxzZSk7XG4gIGNvbnN0IFtpc0xvYWRpbmcsIHNldElzTG9hZGluZ10gPSB1c2VTdGF0ZSh0cnVlKTtcblxuICB1c2VFZmZlY3QoKCkgPT4ge1xuICAgIHNldFRpbWUodGltZUR1cmF0aW9uKVxuICAgIHNldEFsbG9jYXRlZFRpbWUodGltZUR1cmF0aW9uKVxuICB9LCBbdGltZUR1cmF0aW9uXSk7XG5cbiAgdXNlRWZmZWN0KCgpID0+IHtcbiAgICBzZXRUaW1lU3RhcnQoc3RhcnRUaW1lKVxuICB9LCBbc3RhcnRUaW1lXSk7XG5cbiAgdXNlRWZmZWN0KCgpID0+IHtcbiAgICBzZXREYXRlKGN1cnJlbnREYXRlVGltZSlcbiAgICBzZXRLZXkoa2V5ICsgMSlcbiAgICBzZXRJc1BsYXlpbmcodHJ1ZSlcbiAgICBzZXRJc0xvYWRpbmcoZmFsc2UpO1xuICAgIGlmIChzdGFydFRpbWUgPiB0aW1lRHVyYXRpb24pICAvLyBJbiBjYXNlIG9mIG92ZXJzaG9vdCBzZXQgdGhlIENvdW50VXBNb2RlPSB0cnVlXG4gICAgICBzZXRDb3VudFVwTW9kZSh0cnVlKVxuICAgIGVsc2UgXG4gICAgICBzZXRDb3VudFVwTW9kZShmYWxzZSlcbiAgfSwgW2N1cnJlbnREYXRlVGltZV0pO1xuXG4gIGNvbnN0IGhhbmRsZUNvbXBsZXRlID0gKCkgPT4ge1xuICAgIHNldENvdW50VXBNb2RlKHRydWUpO1xuICAgIHJldHVybiB7IHNob3VsZFJlcGVhdDogZmFsc2UgfTsgLy8gcHJldmVudCBhdXRvLXJlcGVhdFxuICB9O1xuXG4gIGNvbnN0IFBhdXNlSWNvbiA9ICh7IGNsYXNzTmFtZSA9IFwidGltZXItaWNvblwiLCBjb2xvciA9ICdjdXJyZW50Q29sb3InIH0pID0+IChcbiAgICA8c3ZnIHhtbG5zPVwiaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmdcIiB2aWV3Qm94PVwiMCAwIDI0IDI0XCIgY2xhc3NOYW1lPXtjbGFzc05hbWV9PlxuICAgICAgPHBhdGggZmlsbD1cIm5vbmVcIiBkPVwiTTAgMGgyNHYyNEgwelwiIC8+XG4gICAgICAgXG4gICAgICA8cGF0aCBkPVwiTTYgNWg0djE0SDZWNXptOCAwaDR2MTRoLTRWNXpcIlxuICAgICAgICBmaWxsPVwidHJhbnNwYXJlbnRcIiAgLy8gTWFrZSBmaWxsIHRyYW5zcGFyZW50XG4gICAgICAgIHN0cm9rZT17Y29sb3J9ICAgICAvLyBBZGQgc3Ryb2tlIGNvbG9yXG4gICAgICAgIHN0cm9rZVdpZHRoPVwiMS41XCIgIC8vIEFkZCBzdHJva2Ugd2lkdGhcbiAgICAgIC8+XG4gICAgPC9zdmc+XG4gICk7XG5cbiAgY29uc3QgU3RhcnRJY29uID0gKHsgY2xhc3NOYW1lID0gXCJ0aW1lci1pY29uXCIsIGNvbG9yID0gJ2N1cnJlbnRDb2xvcicgfSkgPT4gKFxuICAgIDxzdmcgeG1sbnM9XCJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2Z1wiIHZpZXdCb3g9XCIwIDAgMjQgMjRcIiBjbGFzc05hbWU9e2NsYXNzTmFtZX0+XG4gICAgICBcbiAgICAgIDxwYXRoIGZpbGw9XCJub25lXCIgZD1cIk0wIDBoMjR2MjRIMHpcIiAvPlxuICAgICAgICA8cGF0aCBkPVwiTTggNXYxNGwxMS03elwiXG4gICAgICAgIGZpbGw9XCJ0cmFuc3BhcmVudFwiICAvLyBNYWtlIGZpbGwgdHJhbnNwYXJlbnRcbiAgICAgICAgc3Ryb2tlPXtjb2xvcn0gICAgIC8vIEFkZCBzdHJva2UgY29sb3JcbiAgICAgICAgc3Ryb2tlV2lkdGg9XCIxLjVcIiAgLy8gQWRkIHN0cm9rZSB3aWR0aFxuXG4gICAgICAvPlxuICAgIDwvc3ZnPlxuICApO1xuXG4gIGNvbnN0IG9uUGF1c2VDbGlja0hhbmRsZXIgPSAoKSA9PiB7XG4gICAgaWYgKGlzUGxheWluZykge1xuICAgICAgaWYgKG9uUGF1c2VDbGlja0FjdGlvbiAmJiBvblBhdXNlQ2xpY2tBY3Rpb24uY2FuRXhlY3V0ZSkge1xuICAgICAgICBvblBhdXNlQ2xpY2tBY3Rpb24uZXhlY3V0ZSgpO1xuICAgICAgfVxuICAgIH1cbiAgICBzZXRJc1BsYXlpbmcoKHByZXYpID0+ICFwcmV2KTtcbiAgfVxuXG4gIGlmICh0aW1lID09IDApIHsgICAgLy8gV2hlbiBBbGxvY2F0aW9uVGltZSA9IDBcbiAgICByZXR1cm4gKFxuICAgICAgPGRpdiBjbGFzc05hbWU9XCJ0aW1lci1jb250YWluZXJcIj5cbiAgICAgICAgPENvdW50ZG93bkNpcmNsZVRpbWVyXG4gICAgICAgICAga2V5PXtrZXl9XG4gICAgICAgICAgaXNQbGF5aW5nPXtpc1BsYXlpbmd9XG4gICAgICAgICAgZHVyYXRpb249e2RlZmF1bHRUaW1lfVxuICAgICAgICAgIHRyYWlsQ29sb3I9e2lzUGxheWluZyA/IFwiIzBGN0VBNVwiIDpcIiM4ODg4ODhcIn1cbiAgICAgICAgICBjb2xvcnM9e2lzUGxheWluZyA/IFwiIzBGN0VBNVwiIDpcIiM4ODg4ODhcIn1cbiAgICAgICAgICBzaXplPXs0MH1cbiAgICAgICAgICBpbml0aWFsUmVtYWluaW5nVGltZT17ZGVmYXVsdFRpbWUgLSB0aW1lU3RhcnR9XG4gICAgICAgICAgc3Ryb2tlV2lkdGg9ezV9XG4gICAgICAgICAgc3Ryb2tlTGluZWNhcD1cInNxdWFyZVwiXG4gICAgICAgICAgb25VcGRhdGU9eyhlbGFwc2VkVGltZSkgPT4ge1xuICAgICAgICAgICAgLy8gVXBkYXRlIE1lbmRpeCB2YWx1ZSBvbiB0aW1lciB1cGRhdGVcbiAgICAgICAgICAgIGlmIChjdXJyZW50VmFsdWU/LnN0YXR1cyA9PT0gXCJhdmFpbGFibGVcIikge1xuICAgICAgICAgICAgICBjb25zdCByID0gcGFyc2VGbG9hdChlbGFwc2VkVGltZSk7XG4gICAgICAgICAgICAgIGNvbnN0IGQgPSBwYXJzZUZsb2F0KGRlZmF1bHRUaW1lKTtcbiAgICAgICAgICAgICAgY3VycmVudFZhbHVlLnNldFRleHRWYWx1ZShcIlwiICsgKChkLXIpLzg2NDAwKS50b0ZpeGVkKDgpKTtcbiAgICAgICAgICAgIH1cbiAgICAgICAgICB9fVxuICAgICAgICA+XG5cbiAgICAgICAgICB7KHsgZWxhcHNlZFRpbWUgfSkgPT4ge1xuXG4gICAgICAgICAgICByZXR1cm4gKFxuICAgICAgICAgICAgICA8c3Bhbj5cbiAgICAgICAgICAgICAgICA8YnV0dG9uIG9uQ2xpY2s9e29uUGF1c2VDbGlja0hhbmRsZXJ9IHN0eWxlPXt7IGJvcmRlcjogJ25vbmUnLCBiYWNrZ3JvdW5kOiAndHJhbnNwYXJlbnQnLCBjdXJzb3I6ICdwb2ludGVyJyB9fSBjbGFzc05hbWU9XCJ0aW1lci1idXR0b25cIj5cbiAgICAgICAgICAgICAgICAgIHtpc1BsYXlpbmcgP1xuICAgICAgICAgICAgICAgICAgICAoPFBhdXNlSWNvbiBjbGFzc05hbWU9XCJ0aW1lci1pY29uLWJ1dHRvblwiIC8+KVxuICAgICAgICAgICAgICAgICAgICA6XG4gICAgICAgICAgICAgICAgICAgICg8U3RhcnRJY29uIGNsYXNzTmFtZT1cInRpbWVyLWljb24tYnV0dG9uXCIgLz4pfVxuICAgICAgICAgICAgICAgIDwvYnV0dG9uPlxuXG4gICAgICAgICAgICAgICAgPHNwYW4gPlxuICAgICAgICAgICAgICAgICAge3JlbmRlclRpbWUoXCJzZWNvbmRzXCIsIGVsYXBzZWRUaW1lLCAwLCBhbGxvY2F0ZWRUaW1lLCBmYWxzZSl9XG4gICAgICAgICAgICAgICAgPC9zcGFuPlxuICAgICAgICAgICAgICA8L3NwYW4+XG4gICAgICAgICAgICApO1xuICAgICAgICAgIH19XG4gICAgICAgIDwvQ291bnRkb3duQ2lyY2xlVGltZXI+XG4gICAgICA8L2Rpdj5cbiAgICApXG4gIH1cbiAgZWxzZSB7XG4gICAgaWYgKGNvdW50VXBNb2RlKSB7ICAgLy8gV2hlbiBBbGxvY2F0aW9uVGltZSA+IDAgJiYgSW4gY2FzZSBvZiBvdmVyc2hvb3RcbiAgICAgIHJldHVybiAoXG4gICAgICAgIDxkaXYgY2xhc3NOYW1lPVwidGltZXItY29udGFpbmVyXCI+XG4gICAgICAgICAgPENvdW50ZG93bkNpcmNsZVRpbWVyXG4gICAgICAgICAgICBrZXk9e2tleX1cbiAgICAgICAgICAgIGlzUGxheWluZz17aXNQbGF5aW5nfVxuICAgICAgICAgICAgZHVyYXRpb249e2RlZmF1bHRUaW1lfVxuICAgICAgICAgICAgdHJhaWxDb2xvcj17XCIjREMwMDAwXCJ9XG4gICAgICAgICAgICBjb2xvcnM9e1wiI0RDMDAwMFwifVxuICAgICAgICAgICAgc2l6ZT17NDB9XG4gICAgICAgICAgICBzdHJva2VXaWR0aD17NX1cbiAgICAgICAgICAgIHN0cm9rZUxpbmVjYXA9XCJzcXVhcmVcIlxuICAgICAgICAgICAgaW5pdGlhbFJlbWFpbmluZ1RpbWU9eyhNYXRoLnJvdW5kKHN0YXJ0VGltZSkgPiBNYXRoLnJvdW5kKHRpbWUpKSBcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA/IChkZWZhdWx0VGltZSAtIHRpbWVTdGFydCkgXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgOiBkZWZhdWx0VGltZX1cbiAgICAgICAgICAgIG9uVXBkYXRlPXsoZWxhcHNlZFRpbWUpID0+IHtcbiAgICAgICAgICAgICAgLy8gVXBkYXRlIE1lbmRpeCB2YWx1ZSBvbiB0aW1lciB1cGRhdGVcbiAgICAgICAgICAgICAgaWYgKGN1cnJlbnRWYWx1ZT8uc3RhdHVzID09PSBcImF2YWlsYWJsZVwiKSB7XG4gICAgICAgICAgICAgICAgY29uc3QgdCA9IHBhcnNlRmxvYXQodGltZSk7XG4gICAgICAgICAgICAgICAgY29uc3QgciA9IHBhcnNlRmxvYXQoZWxhcHNlZFRpbWUpO1xuICAgICAgICAgICAgICAgIGNvbnN0IGQgPSBwYXJzZUZsb2F0KGRlZmF1bHRUaW1lKTtcbiAgICAgICAgICAgICAgICBpZihNYXRoLnJvdW5kKHN0YXJ0VGltZSkgPiBNYXRoLnJvdW5kKHRpbWUpKVxuICAgICAgICAgICAgICAgICAgIGN1cnJlbnRWYWx1ZS5zZXRUZXh0VmFsdWUoXCJcIiArICgoZC1yKS84NjQwMCkudG9GaXhlZCg4KSk7XG4gICAgICAgICAgICAgICAgZWxzZVxuICAgICAgICAgICAgICAgICAgY3VycmVudFZhbHVlLnNldFRleHRWYWx1ZShcIlwiICsgKChkLXIpLzg2NDAwKS50b0ZpeGVkKDgpKTtcbiAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgfX1cbiAgICAgICAgICA+XG5cbiAgICAgICAgICAgIHsoeyBlbGFwc2VkVGltZSB9KSA9PiB7XG5cbiAgICAgICAgICAgICAgcmV0dXJuIChcbiAgICAgICAgICAgICAgICA8c3Bhbj5cbiAgICAgICAgICAgICAgICAgIDxidXR0b24gb25DbGljaz17b25QYXVzZUNsaWNrSGFuZGxlcn0gc3R5bGU9e3sgYm9yZGVyOiAnbm9uZScsIGJhY2tncm91bmQ6ICd0cmFuc3BhcmVudCcsIGN1cnNvcjogJ3BvaW50ZXInIH19IGNsYXNzTmFtZT1cInRpbWVyLWJ1dHRvblwiPlxuICAgICAgICAgICAgICAgICAgICB7aXNQbGF5aW5nID9cbiAgICAgICAgICAgICAgICAgICAgICAoPFBhdXNlSWNvbiBjbGFzc05hbWU9XCJ0aW1lci1pY29uLWJ1dHRvblwiIC8+KVxuICAgICAgICAgICAgICAgICAgICAgIDpcbiAgICAgICAgICAgICAgICAgICAgICAoPFN0YXJ0SWNvbiBjbGFzc05hbWU9XCJ0aW1lci1pY29uLWJ1dHRvblwiIC8+KX1cbiAgICAgICAgICAgICAgICAgIDwvYnV0dG9uPlxuXG4gICAgICAgICAgICAgICAgICA8c3BhbiA+XG4gICAgICAgICAgICAgICAgICAgIHtyZW5kZXJUaW1lKFwic2Vjb25kc1wiLCAoZWxhcHNlZFRpbWUgPiB0aW1lKT8oZWxhcHNlZFRpbWUtdGltZSk6ZWxhcHNlZFRpbWUsIDAsIGFsbG9jYXRlZFRpbWUsdHJ1ZSl9XG4gICAgICAgICAgICAgICAgICA8L3NwYW4+XG4gICAgICAgICAgICAgICAgPC9zcGFuPlxuICAgICAgICAgICAgICApO1xuICAgICAgICAgICAgfX1cbiAgICAgICAgICA8L0NvdW50ZG93bkNpcmNsZVRpbWVyPlxuICAgICAgICA8L2Rpdj5cbiAgICAgIClcbiAgICB9XG4gICAgZWxzZSB7ICAgICAgICAgICAgICAvLyBXaGVuIEFsbG9jYXRpb25UaW1lID4gMFxuICAgICAgcmV0dXJuIChcbiAgICAgICAgPGRpdiBjbGFzc05hbWU9XCJ0aW1lci1jb250YWluZXJcIj5cbiAgICAgICAgICA8Q291bnRkb3duQ2lyY2xlVGltZXJcbiAgICAgICAgICAgIGtleT17a2V5fVxuICAgICAgICAgICAgaXNQbGF5aW5nPXtpc1BsYXlpbmd9XG4gICAgICAgICAgICBkdXJhdGlvbj17dGltZX1cbiAgICAgICAgICAgIGNvbG9ycz17aXNQbGF5aW5nID8gW1wiIzBGN0VBNVwiLCBcIiMwRjdFQTVcIiwgXCIjMEY3RUE1XCIsIFwiIzBGN0VBNVwiXSA6IFwiIzg4ODg4OFwifVxuICAgICAgICAgICAgY29sb3JzVGltZT17W3RpbWUsIHRpbWUgLyAyLCAzMCwgMF19XG4gICAgICAgICAgICBzaXplPXs0MH1cbiAgICAgICAgICAgIGluaXRpYWxSZW1haW5pbmdUaW1lPXt0aW1lU3RhcnR9XG4gICAgICAgICAgICBzdHJva2VXaWR0aD17NX1cbiAgICAgICAgICAgIHRyYWlsQ29sb3I9e2lzUGxheWluZyA/IFwiI0MwRDNEQlwiIDogXCIjY2NjY2NjXCJ9XG4gICAgICAgICAgICBzdHJva2VMaW5lY2FwPVwic3F1YXJlXCJcbiAgICAgICAgICAgIG9uQ29tcGxldGU9e2hhbmRsZUNvbXBsZXRlfVxuICAgICAgICAgID5cblxuXG4gICAgICAgICAgICB7KHsgcmVtYWluaW5nVGltZSB9KSA9PiB7XG5cbiAgICAgICAgICAgICAgaWYgKHh4ICE9PSByZW1haW5pbmdUaW1lKSB7XG4gICAgICAgICAgICAgICAgc2V0VGltZW91dCgoKSA9PiBzZXRYeChyZW1haW5pbmdUaW1lKSwgMCk7IC8vIGF2b2lkIHNldHRpbmcgc3RhdGUgZHVyaW5nIHJlbmRlclxuICAgICAgICAgICAgICAgIGNvbnN0IHIgPSBwYXJzZUZsb2F0KHJlbWFpbmluZ1RpbWUpO1xuICAgICAgICAgICAgICAgIGNvbnN0IGQgPSBwYXJzZUZsb2F0KHRpbWUpO1xuICAgICAgICAgICAgICAgIGN1cnJlbnRWYWx1ZS5zZXRUZXh0VmFsdWUoXCJcIiArICgoZC1yKS84NjQwMCkudG9GaXhlZCg4KSk7XG4gICAgICAgICAgICAgIH1cblxuICAgICAgICAgICAgICByZXR1cm4gKFxuICAgICAgICAgICAgICAgIDxzcGFuPlxuICAgICAgICAgICAgICAgICAgPGJ1dHRvbiBvbkNsaWNrPXtvblBhdXNlQ2xpY2tIYW5kbGVyfSBzdHlsZT17eyBib3JkZXI6ICdub25lJywgYmFja2dyb3VuZDogJ3RyYW5zcGFyZW50JywgY3Vyc29yOiAncG9pbnRlcicgfX0gY2xhc3NOYW1lPVwidGltZXItYnV0dG9uXCI+XG4gICAgICAgICAgICAgICAgICAgIHtpc1BsYXlpbmcgPyA8UGF1c2VJY29uIGNsYXNzTmFtZT1cInRpbWVyLWljb24tYnV0dG9uXCIgLz4gOiA8U3RhcnRJY29uIGNsYXNzTmFtZT1cInRpbWVyLWljb24tYnV0dG9uXCIgLz59XG4gICAgICAgICAgICAgICAgICA8L2J1dHRvbj5cbiAgICAgICAgICAgICAgICAgIDxzcGFuPlxuICAgICAgICAgICAgICAgICAgICB7cmVuZGVyVGltZShcInNlY29uZHNcIiwgcmVtYWluaW5nVGltZSwgdGltZSwgYWxsb2NhdGVkVGltZSwgZmFsc2UpfVxuICAgICAgICAgICAgICAgICAgPC9zcGFuPlxuICAgICAgICAgICAgICAgIDwvc3Bhbj5cbiAgICAgICAgICAgICAgKTtcbiAgICAgICAgICAgIH19XG5cbiAgICAgICAgICA8L0NvdW50ZG93bkNpcmNsZVRpbWVyPlxuICAgICAgICA8L2Rpdj5cbiAgICAgIClcbiAgICB9XG5cbiAgfVxufVxuIiwiaW1wb3J0IHsgY3JlYXRlRWxlbWVudCwgdXNlU3RhdGUsIHVzZUVmZmVjdCwgbWVtbyB9IGZyb20gXCJyZWFjdFwiO1xuXG5pbXBvcnQgeyBDb3VudGRvd25UaW1lckNvbXBvbmVudCB9IGZyb20gXCIuL2NvbXBvbmVudHMvQ291bnRkb3duVGltZXJDb21wb25lbnRcIjtcbmltcG9ydCBcIi4vdWkvQ291bnRkb3duVGltZXIuY3NzXCI7XG5cbi8vIEhlbHBlciBmdW5jdGlvbiB0byBjb21wYXJlIE1lbmRpeC1saWtlIG9iamVjdHMgKHN0YXR1cyBhbmQgdmFsdWUpXG5jb25zdCBhcmVNZW5kaXhQcm9wc0VxdWFsID0gKHByZXYsIG5leHQpID0+IHtcbiAgaWYgKCFwcmV2IHx8ICFuZXh0KSByZXR1cm4gcHJldiA9PT0gbmV4dDtcbiAgaWYgKHByZXYuc3RhdHVzICE9PSBuZXh0LnN0YXR1cykgcmV0dXJuIGZhbHNlO1xuXG4gIGlmIChwcmV2LnZhbHVlIGluc3RhbmNlb2YgRGF0ZSAmJiBuZXh0LnZhbHVlIGluc3RhbmNlb2YgRGF0ZSkge1xuICAgIHJldHVybiBwcmV2LnZhbHVlLmdldFRpbWUoKSA9PT0gbmV4dC52YWx1ZS5nZXRUaW1lKCk7XG4gIH1cbiAgLy8gRm9yIG51bWJlcnMsIHN0cmluZ3MsIGV0Yy5cbiAgcmV0dXJuIHByZXYudmFsdWUgPT09IG5leHQudmFsdWU7XG59O1xuXG4vLyBDdXN0b20gY29tcGFyaXNvbiBmdW5jdGlvbiBmb3IgUmVhY3QubWVtb1xuY29uc3QgYXJlQ291bnRkb3duUHJvcHNFcXVhbCA9IChwcmV2UHJvcHMsIG5leHRQcm9wcykgPT4ge1xuICAvLyBDb21wYXJlIGVhY2ggTWVuZGl4LWxpa2UgcHJvcCB1c2luZyB5b3VyIGhlbHBlclxuICBjb25zdCBjdXJyZW50RGF0ZVRpbWVDaGFuZ2VkID0gIWFyZU1lbmRpeFByb3BzRXF1YWwocHJldlByb3BzLmN1cnJlbnREYXRlVGltZSwgbmV4dFByb3BzLmN1cnJlbnREYXRlVGltZSk7XG4gIC8vIElmIHRydWUsIGl0IG1lYW5zIGEgcHJvcCBoYXMgY2hhbmdlZCwgc28gcmV0dXJuIGZhbHNlIChtZWFuaW5nIHJlLXJlbmRlcilcbiAgaWYgKGN1cnJlbnREYXRlVGltZUNoYW5nZWQpIHtcbiAgICByZXR1cm4gZmFsc2U7XG4gIH1cbiAgLy8gSWYgbm8gcmVsZXZhbnQgcHJvcCBoYXMgY2hhbmdlZCwgcmV0dXJuIHRydWUgKG1lYW5pbmcgRE8gTk9UIHJlLXJlbmRlcilcbiAgcmV0dXJuIHRydWU7XG59O1xuXG5cbmV4cG9ydCBjb25zdCBDb3VudGRvd25UaW1lciA9IG1lbW8oZnVuY3Rpb24gQ291bnRkb3duVGltZXIoe1xuICB0aW1lRHVyYXRpb24sXG4gIGN1cnJlbnRWYWx1ZSxcbiAgc3RhcnRUaW1lLFxuICBvblBhdXNlQ2xpY2tBY3Rpb24sXG4gIGN1cnJlbnREYXRlVGltZVxufSkge1xuICBjb25zdCBbdGltZSwgc2V0VGltZV0gPSB1c2VTdGF0ZSh0aW1lRHVyYXRpb24pO1xuICBjb25zdCBbYywgc2V0Q10gPSB1c2VTdGF0ZShjdXJyZW50VmFsdWUpO1xuICBjb25zdCBbdGltZVN0YXJ0LCBzZXRUaW1lU3RhcnRdID0gdXNlU3RhdGUoc3RhcnRUaW1lKTtcbiAgY29uc3QgW2RhdGUsIHNldERhdGVdID0gdXNlU3RhdGUoY3VycmVudERhdGVUaW1lKTtcblxuICB1c2VFZmZlY3QoKCkgPT4ge1xuICAgIC8vIFRoaXMgd2lsbCBvbmx5IHJ1biBpZiB0aW1lRHVyYXRpb24gKHByb3ApIGNoYW5nZXMgQU5EIFJlYWN0Lm1lbW8gYWxsb3dlZCBhIHJlLXJlbmRlclxuICAgIHNldFRpbWUodGltZUR1cmF0aW9uKTtcbiAgfSwgW3RpbWVEdXJhdGlvbl0pO1xuXG4gIHVzZUVmZmVjdCgoKSA9PiB7XG4gICAgc2V0VGltZVN0YXJ0KHN0YXJ0VGltZSk7XG4gIH0sIFtzdGFydFRpbWVdKTtcblxuICB1c2VFZmZlY3QoKCkgPT4ge1xuICAgIHNldEMoY3VycmVudFZhbHVlKTtcbiAgfSwgW2N1cnJlbnRWYWx1ZV0pO1xuXG4gIHVzZUVmZmVjdCgoKSA9PiB7XG4gICAgc2V0RGF0ZShjdXJyZW50RGF0ZVRpbWUpO1xuICB9LCBbY3VycmVudERhdGVUaW1lXSk7XG5cbiAgaWYgKHRpbWUuc3RhdHVzICE9PSBcImF2YWlsYWJsZVwiIHx8IGMuc3RhdHVzICE9PSBcImF2YWlsYWJsZVwiIHx8IHRpbWVTdGFydC5zdGF0dXMgIT09IFwiYXZhaWxhYmxlXCIgfHwgZGF0ZS5zdGF0dXMgIT09IFwiYXZhaWxhYmxlXCIpIHtcbiAgICByZXR1cm4gPGRpdj5Mb2FkaW5nLi4uPC9kaXY+O1xuICB9IGVsc2Uge1xuICAgIHJldHVybiAoXG4gICAgICA8Q291bnRkb3duVGltZXJDb21wb25lbnRcbiAgICAgICAgdGltZUR1cmF0aW9uPXt0aW1lLnZhbHVlLnRvTnVtYmVyKCl9XG4gICAgICAgIGN1cnJlbnRWYWx1ZT17Y31cbiAgICAgICAgc3RhcnRUaW1lPXt0aW1lU3RhcnQudmFsdWUudG9OdW1iZXIoKX1cbiAgICAgICAgb25QYXVzZUNsaWNrQWN0aW9uPXtvblBhdXNlQ2xpY2tBY3Rpb259XG4gICAgICAgIGN1cnJlbnREYXRlVGltZT17ZGF0ZS52YWx1ZX1cbiAgICAgIC8+XG4gICAgKTtcbiAgfVxufSwgYXJlQ291bnRkb3duUHJvcHNFcXVhbCk7IC8vIDwtLSBQYXNzIHRoZSBjdXN0b20gY29tcGFyaXNvbiBmdW5jdGlvbiBoZXJlIl0sIm5hbWVzIjpbIkciLCJ3aW5kb3ciLCJNIiwiTCIsIkkiLCJpc1BsYXlpbmciLCJvIiwiZHVyYXRpb24iLCJlIiwic3RhcnRBdCIsIm4iLCJ1cGRhdGVJbnRlcnZhbCIsInQiLCJvbkNvbXBsZXRlIiwicyIsIm9uVXBkYXRlIiwiciIsImkiLCJjIiwiRSIsIm0iLCJiIiwicCIsImYiLCJ1IiwiYSIsImgiLCJ3IiwiZyIsImwiLCJjdXJyZW50IiwicmVxdWVzdEFuaW1hdGlvbkZyYW1lIiwiZCIsIkMiLCJrIiwiUiIsInYiLCIkIiwiY2FuY2VsQW5pbWF0aW9uRnJhbWUiLCJjbGVhclRpbWVvdXQiLCJ5IiwicSIsInNob3VsZFJlcGVhdCIsImRlbGF5IiwibmV3U3RhcnRBdCIsInNldFRpbWVvdXQiLCJlbGFwc2VkVGltZSIsInJlc2V0IiwiQSIsIk1hdGgiLCJQSSIsInBhdGgiLCJwYXRoTGVuZ3RoIiwiVCIsIkIiLCJwb3NpdGlvbiIsIndpZHRoIiwiaGVpZ2h0IiwiUCIsImRpc3BsYXkiLCJqdXN0aWZ5Q29udGVudCIsImFsaWduSXRlbXMiLCJsZWZ0IiwidG9wIiwiRiIsIlciLCJyZXBsYWNlIiwic3Vic3RyaW5nIiwibWF0Y2giLCJtYXAiLCJwYXJzZUludCIsImoiLCJjb2xvcnMiLCJjb2xvcnNUaW1lIiwiaXNTbW9vdGhDb2xvclRyYW5zaXRpb24iLCJmaW5kSW5kZXgiLCJpc0dyb3dpbmciLCJqb2luIiwiUyIsImluaXRpYWxSZW1haW5pbmdUaW1lIiwic2l6ZSIsInN0cm9rZVdpZHRoIiwidHJhaWxTdHJva2VXaWR0aCIsInJvdGF0aW9uIiwiVSIsIm1heCIsImNlaWwiLCJuZXdJbml0aWFsUmVtYWluaW5nVGltZSIsInJlbWFpbmluZ1RpbWUiLCJzdHJva2UiLCJzdHJva2VEYXNob2Zmc2V0IiwiRCIsImNoaWxkcmVuIiwic3Ryb2tlTGluZWNhcCIsInRyYWlsQ29sb3IiLCJ4IiwiY3JlYXRlRWxlbWVudCIsInN0eWxlIiwidmlld0JveCIsInhtbG5zIiwiZmlsbCIsInN0cm9rZURhc2hhcnJheSIsImNvbG9yIiwiZGlzcGxheU5hbWUiLCJyZW5kZXJUaW1lIiwiZGltZW5zaW9uIiwidGltZSIsInRpbWVEdXJhdGlvbiIsImFsbG9jYXRlZFRpbWUiLCJpc092ZXJzaG9vdCIsInJvdW5kIiwibWludXRlcyIsImZsb29yIiwidG9TdHJpbmciLCJwYWRTdGFydCIsInNlY29uZHMiLCJ0cnVuYyIsImZvcm1hdGVkSG91cnMiLCJmb3JtYXRlZG1pbnV0ZXMiLCJjbGFzc05hbWUiLCJib3R0b20iLCJDb3VudGRvd25UaW1lckNvbXBvbmVudCIsImN1cnJlbnRWYWx1ZSIsInN0YXJ0VGltZSIsIm9uUGF1c2VDbGlja0FjdGlvbiIsImN1cnJlbnREYXRlVGltZSIsInNldElzUGxheWluZyIsInVzZVN0YXRlIiwic2V0VGltZSIsInRpbWVTdGFydCIsInNldFRpbWVTdGFydCIsImtleSIsInNldEtleSIsInh4Iiwic2V0WHgiLCJkYXRlIiwic2V0RGF0ZSIsImRlZmF1bHRUaW1lIiwic2V0RGVmYXVsdFRpbWUiLCJOdW1iZXIiLCJNQVhfU0FGRV9JTlRFR0VSIiwic2V0QWxsb2NhdGVkVGltZSIsImNvdW50VXBNb2RlIiwic2V0Q291bnRVcE1vZGUiLCJpc0xvYWRpbmciLCJzZXRJc0xvYWRpbmciLCJ1c2VFZmZlY3QiLCJoYW5kbGVDb21wbGV0ZSIsIlBhdXNlSWNvbiIsIlN0YXJ0SWNvbiIsIm9uUGF1c2VDbGlja0hhbmRsZXIiLCJjYW5FeGVjdXRlIiwiZXhlY3V0ZSIsInByZXYiLCJDb3VudGRvd25DaXJjbGVUaW1lciIsInN0YXR1cyIsInBhcnNlRmxvYXQiLCJzZXRUZXh0VmFsdWUiLCJ0b0ZpeGVkIiwib25DbGljayIsImJvcmRlciIsImJhY2tncm91bmQiLCJjdXJzb3IiLCJhcmVNZW5kaXhQcm9wc0VxdWFsIiwibmV4dCIsInZhbHVlIiwiRGF0ZSIsImdldFRpbWUiLCJhcmVDb3VudGRvd25Qcm9wc0VxdWFsIiwicHJldlByb3BzIiwibmV4dFByb3BzIiwiY3VycmVudERhdGVUaW1lQ2hhbmdlZCIsIkNvdW50ZG93blRpbWVyIiwibWVtbyIsInNldEMiLCJ0b051bWJlciJdLCJtYXBw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
