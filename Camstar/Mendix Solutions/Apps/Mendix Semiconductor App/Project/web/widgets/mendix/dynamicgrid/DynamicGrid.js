define(['exports', 'react'], (function (exports, react) { 'use strict';

    function formatCell(value, format) {
      if (value == null) return "";
      if (!format) return String(value);
      const f = String(format).toLowerCase();
      if (f === "currency") {
        const num = Number(value);
        if (Number.isNaN(num)) return String(value);
        return new Intl.NumberFormat(undefined, {
          style: "currency",
          currency: "USD"
        }).format(num);
      }
      if (f === "number") {
        const num = Number(value);
        return Number.isNaN(num) ? String(value) : String(num);
      }
      if (f === "date") {
        const d = new Date(value);
        return isNaN(d.getTime()) ? String(value) : d.toLocaleString();
      }
      if (f === "uppercase") return String(value).toUpperCase();
      if (f === "lowercase") return String(value).toLowerCase();
      return String(value);
    }
    function Table({
      columns,
      rows,
      formats,
      onSort,
      sortState,
      allowSorting,
      consolidate
    }) {
      const handleHeaderClick = col => {
        if (!allowSorting) return;
        if (!onSort) return;
        const {
          column,
          direction
        } = sortState || {};
        let nextDir = "asc";
        if (column === col && direction === "asc") nextDir = "desc";
        onSort({
          column: col,
          direction: nextDir
        });
      };
      return react.createElement("table", {
        className: "dynamic-grid",
        role: "table"
      }, react.createElement("thead", null, react.createElement("tr", null, columns.map(col => react.createElement("th", {
        key: col,
        onClick: () => handleHeaderClick(col),
        className: allowSorting ? "sortable" : ""
      }, col, allowSorting && sortState && sortState.column === col ? sortState.direction === "asc" ? " \u25B2" : " \u25BC" : "")))), react.createElement("tbody", null, (() => {
        // If consolidation with rowspan is requested, compute spans per column
        const n = rows.length;
        const spansByCol = {};
        columns.forEach(col => {
          const spans = new Array(n).fill(0);
          if (!consolidate) {
            for (let i = 0; i < n; i++) spans[i] = 1;
            spansByCol[col] = spans;
            return;
          }
          let i = 0;
          while (i < n) {
            const startVal = rows[i][col];
            let span = 1;
            let j = i + 1;
            while (j < n && rows[j][col] === startVal) {
              span++;
              j++;
            }
            spans[i] = span;
            for (let k = i + 1; k < i + span; k++) spans[k] = 0; // covered by rowspan
            i = j;
          }
          spansByCol[col] = spans;
        });
        return rows.map((row, ri) => react.createElement("tr", {
          key: ri
        }, columns.map(col => {
          const span = spansByCol[col][ri];
          if (!span) return null; // covered by previous rowspan
          const raw = row[col];
          return react.createElement("td", {
            key: col,
            rowSpan: span > 1 ? span : undefined
          }, formatCell(raw, formats && formats[col]));
        })));
      })()));
    }
    function HelloWorldSample({
      sampleText,
      jsonString,
      allowSorting = true,
      searchable = true,
      consolidate = false,
      columnFormats,
      searchPlaceholder,
      showPagination = false,
      pageSize = 10,
      enableCsvExport = false,
      exportFileName = "export",
      accentColor = "#60a5fa",
      accentColor2 = "#6ee7b7",
      blurAmount = 8,
      sortedColumnTint = "rgba(96,165,250,0.06)",
      enableSortTransitions = true,
      density = "comfortable"
    }) {
      // Fallback no-data
      if (!jsonString) {
        return react.createElement("div", {
          className: "widget-hello-world"
        }, "Hello ", sampleText);
      }

      // Accept either a raw JSON string, an already-parsed array/object,
      // or an object that contains a string attribute (common in Mendix contexts).
      let parsed;
      let rawInput = jsonString;
      if (rawInput && typeof rawInput === "object" && !Array.isArray(rawInput)) {
        // look for common attribute keys that might contain the JSON string
        const candidateKeys = ["json", "value", "data", "text"];
        for (const k of candidateKeys) {
          if (Object.prototype.hasOwnProperty.call(rawInput, k)) {
            const v = rawInput[k];
            if (typeof v === "string") {
              rawInput = v;
              break;
            }
            // some Mendix attribute objects can be { value: "..." }
            if (v && typeof v === "object" && typeof v.value === "string") {
              rawInput = v.value;
              break;
            }
          }
        }
      }
      try {
        parsed = typeof rawInput === "string" ? JSON.parse(rawInput) : rawInput;
      } catch (err) {
        return react.createElement("div", {
          className: "widget-hello-world"
        }, "Invalid JSON provided");
      }
      if (!Array.isArray(parsed) || parsed.length === 0) {
        // support single object
        if (parsed && typeof parsed === "object") parsed = [parsed];else return react.createElement("div", {
          className: "widget-hello-world"
        }, "No rows to display");
      }

      // Support key/value pair arrays: [{ Column: 'col1', Value: 'v1', Row?: 0 }, ...]
      if (Array.isArray(parsed) && parsed.length > 0 && typeof parsed[0] === "object" && Object.prototype.hasOwnProperty.call(parsed[0], "Column") && Object.prototype.hasOwnProperty.call(parsed[0], "Value")) {
        const rowsMap = {};
        parsed.forEach(item => {
          const col = item.Column;
          const val = item.Value;
          const rowKey = item.Row != null ? String(item.Row) : "0";
          if (!rowsMap[rowKey]) rowsMap[rowKey] = {};
          rowsMap[rowKey][col] = val;
        });
        const rowKeys = Object.keys(rowsMap).sort((a, b) => Number(a) - Number(b));
        parsed = rowKeys.map(k => rowsMap[k]);
      }

      // Parse column formats mapping if provided
      let formats = null;
      if (columnFormats) {
        try {
          formats = typeof columnFormats === "string" ? JSON.parse(columnFormats) : columnFormats;
        } catch (e) {
          formats = null;
        }
      }

      // Derive columns from first row (assume consistent keys)
      const derivedColumns = react.useMemo(() => Object.keys(parsed[0] || {}), [parsed]);

      // Visible columns (do not hide columns when consolidating; consolidation suppresses repeated cell values)
      const visibleColumns = react.useMemo(() => derivedColumns, [derivedColumns]);

      // Search state
      const [search, setSearch] = react.useState("");
      const lowerSearch = String(search || "").toLowerCase();
      const filteredRows = react.useMemo(() => {
        if (!searchable || !search) return parsed;
        return parsed.filter(r => {
          return visibleColumns.some(c => {
            const v = r[c];
            return v != null && String(v).toLowerCase().includes(lowerSearch);
          });
        });
      }, [parsed, search, searchable, visibleColumns, lowerSearch]);

      // Sorting state
      const [sortState, setSortState] = react.useState(null);
      const sortedRows = react.useMemo(() => {
        if (!sortState || !sortState.column) return filteredRows;
        const col = sortState.column;
        const dir = sortState.direction === "desc" ? -1 : 1;
        const rowsCopy = [...filteredRows];
        rowsCopy.sort((a, b) => {
          const va = a[col];
          const vb = b[col];
          if (va == null && vb == null) return 0;
          if (va == null) return -1 * dir;
          if (vb == null) return 1 * dir;
          // numeric compare if both numbers
          const na = Number(va);
          const nb = Number(vb);
          if (!Number.isNaN(na) && !Number.isNaN(nb)) return (na - nb) * dir;
          return String(va).localeCompare(String(vb)) * dir;
        });
        return rowsCopy;
      }, [filteredRows, sortState]);

      // Pagination settings
      const effectivePageSize = pageSize && Number(pageSize) > 0 ? Number(pageSize) : 10;
      const [page, setPage] = react.useState(1);
      const totalRows = sortedRows.length;
      const totalPages = Math.max(1, Math.ceil(totalRows / effectivePageSize));

      // Clamp page
      if (page > totalPages) setPage(totalPages);
      const pageStart = (page - 1) * effectivePageSize;
      const pageRows = showPagination ? sortedRows.slice(pageStart, pageStart + effectivePageSize) : sortedRows;

      // CSV export (exports filtered+sorted rows, not just current page)
      const exportToCsv = fileName => {
        const rowsToExport = sortedRows;
        if (!rowsToExport || rowsToExport.length === 0) return;
        const cols = visibleColumns;
        const header = cols.join(",");
        const esc = v => {
          if (v == null) return "";
          const s = String(v);
          // escape double quotes, wrap with quotes if contains comma/quote/newline
          const needsWrap = /[",\n]/.test(s);
          const escaped = s.replace(/"/g, '""');
          return needsWrap ? `"${escaped}"` : escaped;
        };
        const lines = [header, ...rowsToExport.map(r => cols.map(c => esc(formatCell(r[c], formats && formats[c]))).join(","))];
        const csv = lines.join("\n");
        const blob = new Blob([csv], {
          type: "text/csv;charset=utf-8;"
        });
        const url = URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.download = `${fileName || "export"}.csv`;
        document.body.appendChild(a);
        a.click();
        a.remove();
        URL.revokeObjectURL(url);
      };
      const themeStyle = {
        ["--accent-2"]: accentColor,
        ["--accent"]: accentColor2,
        ["--blur-amount"]: `${blurAmount}px`,
        ["--sorted-tint"]: sortedColumnTint,
        ["--row-padding"]: density === "compact" ? "6px" : "12px"
      };
      return react.createElement("div", {
        className: `widget-hello-world ${enableSortTransitions ? "enable-transitions" : ""} tw-widget tw-bg-white tw-rounded-lg tw-shadow-sm tw-text-gray-800 ${density === "compact" ? "tw-dense" : ""}`,
        style: themeStyle
      }, searchable ? react.createElement("div", {
        className: "grid-controls"
      }, react.createElement("input", {
        className: "search-input tw-input",
        placeholder: searchPlaceholder || "Search...",
        value: search,
        onChange: e => setSearch(e.target.value)
      })) : null, react.createElement("div", {
        className: "grid-action-row"
      }, enableCsvExport ? react.createElement("button", {
        className: "export-btn tw-btn",
        onClick: () => exportToCsv(exportFileName || "export")
      }, "Export CSV") : null), react.createElement("div", {
        className: "table-wrapper"
      }, react.createElement(Table, {
        columns: visibleColumns,
        rows: pageRows,
        formats: formats,
        allowSorting: allowSorting,
        sortState: sortState,
        onSort: s => setSortState(s),
        consolidate: consolidate
      })), showPagination ? react.createElement("div", {
        className: "pagination-controls"
      }, react.createElement("button", {
        onClick: () => setPage(p => Math.max(1, p - 1)),
        disabled: page <= 1
      }, "\xAB Prev"), react.createElement("span", {
        className: "page-info"
      }, "Page ", page, " / ", totalPages), react.createElement("button", {
        onClick: () => setPage(p => Math.min(totalPages, p + 1)),
        disabled: page >= totalPages
      }, "Next \xBB")) : null);
    }

    function DynamicGrid({
      sampleText,
      jsonString,
      allowSorting,
      searchable,
      consolidate,
      columnFormats,
      searchPlaceholder,
      showPagination,
      pageSize,
      enableCsvExport,
      exportFileName,
      dense
    }) {
      const density = dense ? "compact" : "comfortable";
      return react.createElement(HelloWorldSample, {
        sampleText: sampleText,
        jsonString: jsonString,
        allowSorting: allowSorting,
        searchable: searchable,
        consolidate: consolidate,
        columnFormats: columnFormats,
        searchPlaceholder: searchPlaceholder,
        showPagination: showPagination,
        pageSize: pageSize,
        enableCsvExport: enableCsvExport,
        exportFileName: exportFileName,
        density: density
      });
    }

    exports.DynamicGrid = DynamicGrid;

}));
//# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiRHluYW1pY0dyaWQuanMiLCJzb3VyY2VzIjpbIi4uLy4uLy4uLy4uLy4uL3NyYy9jb21wb25lbnRzL0hlbGxvV29ybGRTYW1wbGUuanN4IiwiLi4vLi4vLi4vLi4vLi4vc3JjL0R5bmFtaWNHcmlkLmpzeCJdLCJzb3VyY2VzQ29udGVudCI6WyJpbXBvcnQgeyBjcmVhdGVFbGVtZW50LCB1c2VNZW1vLCB1c2VTdGF0ZSB9IGZyb20gXCJyZWFjdFwiO1xuXG5mdW5jdGlvbiBmb3JtYXRDZWxsKHZhbHVlLCBmb3JtYXQpIHtcbiAgICBpZiAodmFsdWUgPT0gbnVsbCkgcmV0dXJuIFwiXCI7XG4gICAgaWYgKCFmb3JtYXQpIHJldHVybiBTdHJpbmcodmFsdWUpO1xuXG4gICAgY29uc3QgZiA9IFN0cmluZyhmb3JtYXQpLnRvTG93ZXJDYXNlKCk7XG4gICAgaWYgKGYgPT09IFwiY3VycmVuY3lcIikge1xuICAgICAgICBjb25zdCBudW0gPSBOdW1iZXIodmFsdWUpO1xuICAgICAgICBpZiAoTnVtYmVyLmlzTmFOKG51bSkpIHJldHVybiBTdHJpbmcodmFsdWUpO1xuICAgICAgICByZXR1cm4gbmV3IEludGwuTnVtYmVyRm9ybWF0KHVuZGVmaW5lZCwgeyBzdHlsZTogXCJjdXJyZW5jeVwiLCBjdXJyZW5jeTogXCJVU0RcIiB9KS5mb3JtYXQobnVtKTtcbiAgICB9XG4gICAgaWYgKGYgPT09IFwibnVtYmVyXCIpIHtcbiAgICAgICAgY29uc3QgbnVtID0gTnVtYmVyKHZhbHVlKTtcbiAgICAgICAgcmV0dXJuIE51bWJlci5pc05hTihudW0pID8gU3RyaW5nKHZhbHVlKSA6IFN0cmluZyhudW0pO1xuICAgIH1cbiAgICBpZiAoZiA9PT0gXCJkYXRlXCIpIHtcbiAgICAgICAgY29uc3QgZCA9IG5ldyBEYXRlKHZhbHVlKTtcbiAgICAgICAgcmV0dXJuIGlzTmFOKGQuZ2V0VGltZSgpKSA/IFN0cmluZyh2YWx1ZSkgOiBkLnRvTG9jYWxlU3RyaW5nKCk7XG4gICAgfVxuICAgIGlmIChmID09PSBcInVwcGVyY2FzZVwiKSByZXR1cm4gU3RyaW5nKHZhbHVlKS50b1VwcGVyQ2FzZSgpO1xuICAgIGlmIChmID09PSBcImxvd2VyY2FzZVwiKSByZXR1cm4gU3RyaW5nKHZhbHVlKS50b0xvd2VyQ2FzZSgpO1xuICAgIHJldHVybiBTdHJpbmcodmFsdWUpO1xufVxuXG5mdW5jdGlvbiBUYWJsZSh7IGNvbHVtbnMsIHJvd3MsIGZvcm1hdHMsIG9uU29ydCwgc29ydFN0YXRlLCBhbGxvd1NvcnRpbmcsIGNvbnNvbGlkYXRlIH0pIHtcbiAgICBjb25zdCBoYW5kbGVIZWFkZXJDbGljayA9IChjb2wpID0+IHtcbiAgICAgICAgaWYgKCFhbGxvd1NvcnRpbmcpIHJldHVybjtcbiAgICAgICAgaWYgKCFvblNvcnQpIHJldHVybjtcbiAgICAgICAgY29uc3QgeyBjb2x1bW4sIGRpcmVjdGlvbiB9ID0gc29ydFN0YXRlIHx8IHt9O1xuICAgICAgICBsZXQgbmV4dERpciA9IFwiYXNjXCI7XG4gICAgICAgIGlmIChjb2x1bW4gPT09IGNvbCAmJiBkaXJlY3Rpb24gPT09IFwiYXNjXCIpIG5leHREaXIgPSBcImRlc2NcIjtcbiAgICAgICAgb25Tb3J0KHsgY29sdW1uOiBjb2wsIGRpcmVjdGlvbjogbmV4dERpciB9KTtcbiAgICB9O1xuXG4gICAgcmV0dXJuIChcbiAgICAgICAgPHRhYmxlIGNsYXNzTmFtZT1cImR5bmFtaWMtZ3JpZFwiIHJvbGU9XCJ0YWJsZVwiPlxuICAgICAgICAgICAgPHRoZWFkPlxuICAgICAgICAgICAgICAgIDx0cj5cbiAgICAgICAgICAgICAgICAgICAge2NvbHVtbnMubWFwKChjb2wpID0+IChcbiAgICAgICAgICAgICAgICAgICAgICAgIDx0aCBrZXk9e2NvbH0gb25DbGljaz17KCkgPT4gaGFuZGxlSGVhZGVyQ2xpY2soY29sKX0gY2xhc3NOYW1lPXthbGxvd1NvcnRpbmcgPyBcInNvcnRhYmxlXCIgOiBcIlwifT5cbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB7Y29sfVxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHthbGxvd1NvcnRpbmcgJiYgc29ydFN0YXRlICYmIHNvcnRTdGF0ZS5jb2x1bW4gPT09IGNvbCA/IChzb3J0U3RhdGUuZGlyZWN0aW9uID09PSBcImFzY1wiID8gXCIgXFx1MjVCMlwiIDogXCIgXFx1MjVCQ1wiKSA6IFwiXCJ9XG4gICAgICAgICAgICAgICAgICAgICAgICA8L3RoPlxuICAgICAgICAgICAgICAgICAgICApKX1cbiAgICAgICAgICAgICAgICA8L3RyPlxuICAgICAgICAgICAgPC90aGVhZD5cbiAgICAgICAgICAgIDx0Ym9keT5cbiAgICAgICAgICAgICAgICB7KCgpID0+IHtcbiAgICAgICAgICAgICAgICAgICAgLy8gSWYgY29uc29saWRhdGlvbiB3aXRoIHJvd3NwYW4gaXMgcmVxdWVzdGVkLCBjb21wdXRlIHNwYW5zIHBlciBjb2x1bW5cbiAgICAgICAgICAgICAgICAgICAgY29uc3QgbiA9IHJvd3MubGVuZ3RoO1xuICAgICAgICAgICAgICAgICAgICBjb25zdCBzcGFuc0J5Q29sID0ge307XG4gICAgICAgICAgICAgICAgICAgIGNvbHVtbnMuZm9yRWFjaCgoY29sKSA9PiB7XG4gICAgICAgICAgICAgICAgICAgICAgICBjb25zdCBzcGFucyA9IG5ldyBBcnJheShuKS5maWxsKDApO1xuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKCFjb25zb2xpZGF0ZSkge1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGZvciAobGV0IGkgPSAwOyBpIDwgbjsgaSsrKSBzcGFuc1tpXSA9IDE7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgc3BhbnNCeUNvbFtjb2xdID0gc3BhbnM7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgcmV0dXJuO1xuICAgICAgICAgICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgICAgICAgICAgICAgbGV0IGkgPSAwO1xuICAgICAgICAgICAgICAgICAgICAgICAgd2hpbGUgKGkgPCBuKSB7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgY29uc3Qgc3RhcnRWYWwgPSByb3dzW2ldW2NvbF07XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgbGV0IHNwYW4gPSAxO1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGxldCBqID0gaSArIDE7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgd2hpbGUgKGogPCBuICYmIHJvd3Nbal1bY29sXSA9PT0gc3RhcnRWYWwpIHtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc3BhbisrO1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBqKys7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNwYW5zW2ldID0gc3BhbjtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBmb3IgKGxldCBrID0gaSArIDE7IGsgPCBpICsgc3BhbjsgaysrKSBzcGFuc1trXSA9IDA7IC8vIGNvdmVyZWQgYnkgcm93c3BhblxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGkgPSBqO1xuICAgICAgICAgICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgICAgICAgICAgICAgc3BhbnNCeUNvbFtjb2xdID0gc3BhbnM7XG4gICAgICAgICAgICAgICAgICAgIH0pO1xuXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiByb3dzLm1hcCgocm93LCByaSkgPT4gKFxuICAgICAgICAgICAgICAgICAgICAgICAgPHRyIGtleT17cml9PlxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHtjb2x1bW5zLm1hcCgoY29sKSA9PiB7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGNvbnN0IHNwYW4gPSBzcGFuc0J5Q29sW2NvbF1bcmldO1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBpZiAoIXNwYW4pIHJldHVybiBudWxsOyAvLyBjb3ZlcmVkIGJ5IHByZXZpb3VzIHJvd3NwYW5cbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgY29uc3QgcmF3ID0gcm93W2NvbF07XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiAoXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8dGQga2V5PXtjb2x9IHJvd1NwYW49e3NwYW4gPiAxID8gc3BhbiA6IHVuZGVmaW5lZH0+XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAge2Zvcm1hdENlbGwocmF3LCBmb3JtYXRzICYmIGZvcm1hdHNbY29sXSl9XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICA8L3RkPlxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICApO1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgIH0pfVxuICAgICAgICAgICAgICAgICAgICAgICAgPC90cj5cbiAgICAgICAgICAgICAgICAgICAgKSk7XG4gICAgICAgICAgICAgICAgfSkoKX1cbiAgICAgICAgICAgIDwvdGJvZHk+XG4gICAgICAgIDwvdGFibGU+XG4gICAgKTtcbn1cblxuZXhwb3J0IGZ1bmN0aW9uIEhlbGxvV29ybGRTYW1wbGUoeyBzYW1wbGVUZXh0LCBqc29uU3RyaW5nLCBhbGxvd1NvcnRpbmcgPSB0cnVlLCBzZWFyY2hhYmxlID0gdHJ1ZSwgY29uc29saWRhdGUgPSBmYWxzZSwgY29sdW1uRm9ybWF0cywgc2VhcmNoUGxhY2Vob2xkZXIsIHNob3dQYWdpbmF0aW9uID0gZmFsc2UsIHBhZ2VTaXplID0gMTAsIGVuYWJsZUNzdkV4cG9ydCA9IGZhbHNlLCBleHBvcnRGaWxlTmFtZSA9IFwiZXhwb3J0XCIsIGFjY2VudENvbG9yID0gXCIjNjBhNWZhXCIsIGFjY2VudENvbG9yMiA9IFwiIzZlZTdiN1wiLCBibHVyQW1vdW50ID0gOCwgc29ydGVkQ29sdW1uVGludCA9IFwicmdiYSg5NiwxNjUsMjUwLDAuMDYpXCIsIGVuYWJsZVNvcnRUcmFuc2l0aW9ucyA9IHRydWUsIGRlbnNpdHkgPSBcImNvbWZvcnRhYmxlXCIgfSkge1xuICAgIC8vIEZhbGxiYWNrIG5vLWRhdGFcbiAgICBpZiAoIWpzb25TdHJpbmcpIHtcbiAgICAgICAgcmV0dXJuIDxkaXYgY2xhc3NOYW1lPVwid2lkZ2V0LWhlbGxvLXdvcmxkXCI+SGVsbG8ge3NhbXBsZVRleHR9PC9kaXY+O1xuICAgIH1cblxuICAgIC8vIEFjY2VwdCBlaXRoZXIgYSByYXcgSlNPTiBzdHJpbmcsIGFuIGFscmVhZHktcGFyc2VkIGFycmF5L29iamVjdCxcbiAgICAvLyBvciBhbiBvYmplY3QgdGhhdCBjb250YWlucyBhIHN0cmluZyBhdHRyaWJ1dGUgKGNvbW1vbiBpbiBNZW5kaXggY29udGV4dHMpLlxuICAgIGxldCBwYXJzZWQ7XG4gICAgbGV0IHJhd0lucHV0ID0ganNvblN0cmluZztcbiAgICBpZiAocmF3SW5wdXQgJiYgdHlwZW9mIHJhd0lucHV0ID09PSBcIm9iamVjdFwiICYmICFBcnJheS5pc0FycmF5KHJhd0lucHV0KSkge1xuICAgICAgICAvLyBsb29rIGZvciBjb21tb24gYXR0cmlidXRlIGtleXMgdGhhdCBtaWdodCBjb250YWluIHRoZSBKU09OIHN0cmluZ1xuICAgICAgICBjb25zdCBjYW5kaWRhdGVLZXlzID0gW1wianNvblwiLCBcInZhbHVlXCIsIFwiZGF0YVwiLCBcInRleHRcIl07XG4gICAgICAgIGZvciAoY29uc3QgayBvZiBjYW5kaWRhdGVLZXlzKSB7XG4gICAgICAgICAgICBpZiAoT2JqZWN0LnByb3RvdHlwZS5oYXNPd25Qcm9wZXJ0eS5jYWxsKHJhd0lucHV0LCBrKSkge1xuICAgICAgICAgICAgICAgIGNvbnN0IHYgPSByYXdJbnB1dFtrXTtcbiAgICAgICAgICAgICAgICBpZiAodHlwZW9mIHYgPT09IFwic3RyaW5nXCIpIHtcbiAgICAgICAgICAgICAgICAgICAgcmF3SW5wdXQgPSB2O1xuICAgICAgICAgICAgICAgICAgICBicmVhaztcbiAgICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICAgLy8gc29tZSBNZW5kaXggYXR0cmlidXRlIG9iamVjdHMgY2FuIGJlIHsgdmFsdWU6IFwiLi4uXCIgfVxuICAgICAgICAgICAgICAgIGlmICh2ICYmIHR5cGVvZiB2ID09PSBcIm9iamVjdFwiICYmIHR5cGVvZiB2LnZhbHVlID09PSBcInN0cmluZ1wiKSB7XG4gICAgICAgICAgICAgICAgICAgIHJhd0lucHV0ID0gdi52YWx1ZTtcbiAgICAgICAgICAgICAgICAgICAgYnJlYWs7XG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgfVxuICAgICAgICB9XG4gICAgfVxuXG4gICAgdHJ5IHtcbiAgICAgICAgcGFyc2VkID0gdHlwZW9mIHJhd0lucHV0ID09PSBcInN0cmluZ1wiID8gSlNPTi5wYXJzZShyYXdJbnB1dCkgOiByYXdJbnB1dDtcbiAgICB9IGNhdGNoIChlcnIpIHtcbiAgICAgICAgcmV0dXJuIDxkaXYgY2xhc3NOYW1lPVwid2lkZ2V0LWhlbGxvLXdvcmxkXCI+SW52YWxpZCBKU09OIHByb3ZpZGVkPC9kaXY+O1xuICAgIH1cblxuICAgIGlmICghQXJyYXkuaXNBcnJheShwYXJzZWQpIHx8IHBhcnNlZC5sZW5ndGggPT09IDApIHtcbiAgICAgICAgLy8gc3VwcG9ydCBzaW5nbGUgb2JqZWN0XG4gICAgICAgIGlmIChwYXJzZWQgJiYgdHlwZW9mIHBhcnNlZCA9PT0gXCJvYmplY3RcIikgcGFyc2VkID0gW3BhcnNlZF07XG4gICAgICAgIGVsc2UgcmV0dXJuIDxkaXYgY2xhc3NOYW1lPVwid2lkZ2V0LWhlbGxvLXdvcmxkXCI+Tm8gcm93cyB0byBkaXNwbGF5PC9kaXY+O1xuICAgIH1cblxuICAgIC8vIFN1cHBvcnQga2V5L3ZhbHVlIHBhaXIgYXJyYXlzOiBbeyBDb2x1bW46ICdjb2wxJywgVmFsdWU6ICd2MScsIFJvdz86IDAgfSwgLi4uXVxuICAgIGlmIChBcnJheS5pc0FycmF5KHBhcnNlZCkgJiYgcGFyc2VkLmxlbmd0aCA+IDAgJiYgdHlwZW9mIHBhcnNlZFswXSA9PT0gXCJvYmplY3RcIiAmJiAoT2JqZWN0LnByb3RvdHlwZS5oYXNPd25Qcm9wZXJ0eS5jYWxsKHBhcnNlZFswXSwgXCJDb2x1bW5cIikgJiYgT2JqZWN0LnByb3RvdHlwZS5oYXNPd25Qcm9wZXJ0eS5jYWxsKHBhcnNlZFswXSwgXCJWYWx1ZVwiKSkpIHtcbiAgICAgICAgY29uc3QgY29sdW1ucyA9IFtdO1xuICAgICAgICBjb25zdCByb3dzTWFwID0ge307XG4gICAgICAgIHBhcnNlZC5mb3JFYWNoKChpdGVtKSA9PiB7XG4gICAgICAgICAgICBjb25zdCBjb2wgPSBpdGVtLkNvbHVtbjtcbiAgICAgICAgICAgIGNvbnN0IHZhbCA9IGl0ZW0uVmFsdWU7XG4gICAgICAgICAgICBjb25zdCByb3dLZXkgPSBpdGVtLlJvdyAhPSBudWxsID8gU3RyaW5nKGl0ZW0uUm93KSA6IFwiMFwiO1xuICAgICAgICAgICAgaWYgKCFjb2x1bW5zLmluY2x1ZGVzKGNvbCkpIGNvbHVtbnMucHVzaChjb2wpO1xuICAgICAgICAgICAgaWYgKCFyb3dzTWFwW3Jvd0tleV0pIHJvd3NNYXBbcm93S2V5XSA9IHt9O1xuICAgICAgICAgICAgcm93c01hcFtyb3dLZXldW2NvbF0gPSB2YWw7XG4gICAgICAgIH0pO1xuICAgICAgICBjb25zdCByb3dLZXlzID0gT2JqZWN0LmtleXMocm93c01hcCkuc29ydCgoYSwgYikgPT4gTnVtYmVyKGEpIC0gTnVtYmVyKGIpKTtcbiAgICAgICAgcGFyc2VkID0gcm93S2V5cy5tYXAoKGspID0+IHJvd3NNYXBba10pO1xuICAgIH1cblxuICAgIC8vIFBhcnNlIGNvbHVtbiBmb3JtYXRzIG1hcHBpbmcgaWYgcHJvdmlkZWRcbiAgICBsZXQgZm9ybWF0cyA9IG51bGw7XG4gICAgaWYgKGNvbHVtbkZvcm1hdHMpIHtcbiAgICAgICAgdHJ5IHtcbiAgICAgICAgICAgIGZvcm1hdHMgPSB0eXBlb2YgY29sdW1uRm9ybWF0cyA9PT0gXCJzdHJpbmdcIiA/IEpTT04ucGFyc2UoY29sdW1uRm9ybWF0cykgOiBjb2x1bW5Gb3JtYXRzO1xuICAgICAgICB9IGNhdGNoIChlKSB7XG4gICAgICAgICAgICBmb3JtYXRzID0gbnVsbDtcbiAgICAgICAgfVxuICAgIH1cblxuICAgIC8vIERlcml2ZSBjb2x1bW5zIGZyb20gZmlyc3Qgcm93IChhc3N1bWUgY29uc2lzdGVudCBrZXlzKVxuICAgIGNvbnN0IGRlcml2ZWRDb2x1bW5zID0gdXNlTWVtbygoKSA9PiBPYmplY3Qua2V5cyhwYXJzZWRbMF0gfHwge30pLCBbcGFyc2VkXSk7XG5cbiAgICAvLyBWaXNpYmxlIGNvbHVtbnMgKGRvIG5vdCBoaWRlIGNvbHVtbnMgd2hlbiBjb25zb2xpZGF0aW5nOyBjb25zb2xpZGF0aW9uIHN1cHByZXNzZXMgcmVwZWF0ZWQgY2VsbCB2YWx1ZXMpXG4gICAgY29uc3QgdmlzaWJsZUNvbHVtbnMgPSB1c2VNZW1vKCgpID0+IGRlcml2ZWRDb2x1bW5zLCBbZGVyaXZlZENvbHVtbnNdKTtcblxuICAgIC8vIFNlYXJjaCBzdGF0ZVxuICAgIGNvbnN0IFtzZWFyY2gsIHNldFNlYXJjaF0gPSB1c2VTdGF0ZShcIlwiKTtcbiAgICBjb25zdCBsb3dlclNlYXJjaCA9IFN0cmluZyhzZWFyY2ggfHwgXCJcIikudG9Mb3dlckNhc2UoKTtcblxuICAgIGNvbnN0IGZpbHRlcmVkUm93cyA9IHVzZU1lbW8oKCkgPT4ge1xuICAgICAgICBpZiAoIXNlYXJjaGFibGUgfHwgIXNlYXJjaCkgcmV0dXJuIHBhcnNlZDtcbiAgICAgICAgcmV0dXJuIHBhcnNlZC5maWx0ZXIoKHIpID0+IHtcbiAgICAgICAgICAgIHJldHVybiB2aXNpYmxlQ29sdW1ucy5zb21lKChjKSA9PiB7XG4gICAgICAgICAgICAgICAgY29uc3QgdiA9IHJbY107XG4gICAgICAgICAgICAgICAgcmV0dXJuIHYgIT0gbnVsbCAmJiBTdHJpbmcodikudG9Mb3dlckNhc2UoKS5pbmNsdWRlcyhsb3dlclNlYXJjaCk7XG4gICAgICAgICAgICB9KTtcbiAgICAgICAgfSk7XG4gICAgfSwgW3BhcnNlZCwgc2VhcmNoLCBzZWFyY2hhYmxlLCB2aXNpYmxlQ29sdW1ucywgbG93ZXJTZWFyY2hdKTtcblxuICAgIC8vIFNvcnRpbmcgc3RhdGVcbiAgICBjb25zdCBbc29ydFN0YXRlLCBzZXRTb3J0U3RhdGVdID0gdXNlU3RhdGUobnVsbCk7XG4gICAgY29uc3Qgc29ydGVkUm93cyA9IHVzZU1lbW8oKCkgPT4ge1xuICAgICAgICBpZiAoIXNvcnRTdGF0ZSB8fCAhc29ydFN0YXRlLmNvbHVtbikgcmV0dXJuIGZpbHRlcmVkUm93cztcbiAgICAgICAgY29uc3QgY29sID0gc29ydFN0YXRlLmNvbHVtbjtcbiAgICAgICAgY29uc3QgZGlyID0gc29ydFN0YXRlLmRpcmVjdGlvbiA9PT0gXCJkZXNjXCIgPyAtMSA6IDE7XG4gICAgICAgIGNvbnN0IHJvd3NDb3B5ID0gWy4uLmZpbHRlcmVkUm93c107XG4gICAgICAgIHJvd3NDb3B5LnNvcnQoKGEsIGIpID0+IHtcbiAgICAgICAgICAgIGNvbnN0IHZhID0gYVtjb2xdO1xuICAgICAgICAgICAgY29uc3QgdmIgPSBiW2NvbF07XG4gICAgICAgICAgICBpZiAodmEgPT0gbnVsbCAmJiB2YiA9PSBudWxsKSByZXR1cm4gMDtcbiAgICAgICAgICAgIGlmICh2YSA9PSBudWxsKSByZXR1cm4gLTEgKiBkaXI7XG4gICAgICAgICAgICBpZiAodmIgPT0gbnVsbCkgcmV0dXJuIDEgKiBkaXI7XG4gICAgICAgICAgICAvLyBudW1lcmljIGNvbXBhcmUgaWYgYm90aCBudW1iZXJzXG4gICAgICAgICAgICBjb25zdCBuYSA9IE51bWJlcih2YSk7XG4gICAgICAgICAgICBjb25zdCBuYiA9IE51bWJlcih2Yik7XG4gICAgICAgICAgICBpZiAoIU51bWJlci5pc05hTihuYSkgJiYgIU51bWJlci5pc05hTihuYikpIHJldHVybiAobmEgLSBuYikgKiBkaXI7XG4gICAgICAgICAgICByZXR1cm4gU3RyaW5nKHZhKS5sb2NhbGVDb21wYXJlKFN0cmluZyh2YikpICogZGlyO1xuICAgICAgICB9KTtcbiAgICAgICAgcmV0dXJuIHJvd3NDb3B5O1xuICAgIH0sIFtmaWx0ZXJlZFJvd3MsIHNvcnRTdGF0ZV0pO1xuXG4gICAgLy8gUGFnaW5hdGlvbiBzZXR0aW5nc1xuICAgIGNvbnN0IGVmZmVjdGl2ZVBhZ2VTaXplID0gcGFnZVNpemUgJiYgTnVtYmVyKHBhZ2VTaXplKSA+IDAgPyBOdW1iZXIocGFnZVNpemUpIDogMTA7XG4gICAgY29uc3QgW3BhZ2UsIHNldFBhZ2VdID0gdXNlU3RhdGUoMSk7XG5cbiAgICBjb25zdCB0b3RhbFJvd3MgPSBzb3J0ZWRSb3dzLmxlbmd0aDtcbiAgICBjb25zdCB0b3RhbFBhZ2VzID0gTWF0aC5tYXgoMSwgTWF0aC5jZWlsKHRvdGFsUm93cyAvIGVmZmVjdGl2ZVBhZ2VTaXplKSk7XG5cbiAgICAvLyBDbGFtcCBwYWdlXG4gICAgaWYgKHBhZ2UgPiB0b3RhbFBhZ2VzKSBzZXRQYWdlKHRvdGFsUGFnZXMpO1xuXG4gICAgY29uc3QgcGFnZVN0YXJ0ID0gKHBhZ2UgLSAxKSAqIGVmZmVjdGl2ZVBhZ2VTaXplO1xuICAgIGNvbnN0IHBhZ2VSb3dzID0gc2hvd1BhZ2luYXRpb24gPyBzb3J0ZWRSb3dzLnNsaWNlKHBhZ2VTdGFydCwgcGFnZVN0YXJ0ICsgZWZmZWN0aXZlUGFnZVNpemUpIDogc29ydGVkUm93cztcblxuICAgIC8vIENTViBleHBvcnQgKGV4cG9ydHMgZmlsdGVyZWQrc29ydGVkIHJvd3MsIG5vdCBqdXN0IGN1cnJlbnQgcGFnZSlcbiAgICBjb25zdCBleHBvcnRUb0NzdiA9IChmaWxlTmFtZSkgPT4ge1xuICAgICAgICBjb25zdCByb3dzVG9FeHBvcnQgPSBzb3J0ZWRSb3dzO1xuICAgICAgICBpZiAoIXJvd3NUb0V4cG9ydCB8fCByb3dzVG9FeHBvcnQubGVuZ3RoID09PSAwKSByZXR1cm47XG4gICAgICAgIGNvbnN0IGNvbHMgPSB2aXNpYmxlQ29sdW1ucztcbiAgICAgICAgY29uc3QgaGVhZGVyID0gY29scy5qb2luKFwiLFwiKTtcbiAgICAgICAgY29uc3QgZXNjID0gKHYpID0+IHtcbiAgICAgICAgICAgIGlmICh2ID09IG51bGwpIHJldHVybiBcIlwiO1xuICAgICAgICAgICAgY29uc3QgcyA9IFN0cmluZyh2KTtcbiAgICAgICAgICAgIC8vIGVzY2FwZSBkb3VibGUgcXVvdGVzLCB3cmFwIHdpdGggcXVvdGVzIGlmIGNvbnRhaW5zIGNvbW1hL3F1b3RlL25ld2xpbmVcbiAgICAgICAgICAgIGNvbnN0IG5lZWRzV3JhcCA9IC9bXCIsXFxuXS8udGVzdChzKTtcbiAgICAgICAgICAgIGNvbnN0IGVzY2FwZWQgPSBzLnJlcGxhY2UoL1wiL2csICdcIlwiJyk7XG4gICAgICAgICAgICByZXR1cm4gbmVlZHNXcmFwID8gYFwiJHtlc2NhcGVkfVwiYCA6IGVzY2FwZWQ7XG4gICAgICAgIH07XG4gICAgICAgIGNvbnN0IGxpbmVzID0gW2hlYWRlciwgLi4ucm93c1RvRXhwb3J0Lm1hcCgocikgPT4gY29scy5tYXAoKGMpID0+IGVzYyhmb3JtYXRDZWxsKHJbY10sIGZvcm1hdHMgJiYgZm9ybWF0c1tjXSkpKS5qb2luKFwiLFwiKSldO1xuICAgICAgICBjb25zdCBjc3YgPSBsaW5lcy5qb2luKFwiXFxuXCIpO1xuICAgICAgICBjb25zdCBibG9iID0gbmV3IEJsb2IoW2Nzdl0sIHsgdHlwZTogXCJ0ZXh0L2NzdjtjaGFyc2V0PXV0Zi04O1wiIH0pO1xuICAgICAgICBjb25zdCB1cmwgPSBVUkwuY3JlYXRlT2JqZWN0VVJMKGJsb2IpO1xuICAgICAgICBjb25zdCBhID0gZG9jdW1lbnQuY3JlYXRlRWxlbWVudChcImFcIik7XG4gICAgICAgIGEuaHJlZiA9IHVybDtcbiAgICAgICAgYS5kb3dubG9hZCA9IGAke2ZpbGVOYW1lIHx8IFwiZXhwb3J0XCJ9LmNzdmA7XG4gICAgICAgIGRvY3VtZW50LmJvZHkuYXBwZW5kQ2hpbGQoYSk7XG4gICAgICAgIGEuY2xpY2soKTtcbiAgICAgICAgYS5yZW1vdmUoKTtcbiAgICAgICAgVVJMLnJldm9rZU9iamVjdFVSTCh1cmwpO1xuICAgIH07XG5cbiAgICBjb25zdCB0aGVtZVN0eWxlID0ge1xuICAgICAgICBbXCItLWFjY2VudC0yXCJdOiBhY2NlbnRDb2xvcixcbiAgICAgICAgW1wiLS1hY2NlbnRcIl06IGFjY2VudENvbG9yMixcbiAgICAgICAgW1wiLS1ibHVyLWFtb3VudFwiXTogYCR7Ymx1ckFtb3VudH1weGAsXG4gICAgICAgIFtcIi0tc29ydGVkLXRpbnRcIl06IHNvcnRlZENvbHVtblRpbnQsXG4gICAgICAgIFtcIi0tcm93LXBhZGRpbmdcIl06IGRlbnNpdHkgPT09IFwiY29tcGFjdFwiID8gXCI2cHhcIiA6IFwiMTJweFwiXG4gICAgfTtcblxuICAgIHJldHVybiAoXG4gICAgICAgIDxkaXYgY2xhc3NOYW1lPXtgd2lkZ2V0LWhlbGxvLXdvcmxkICR7ZW5hYmxlU29ydFRyYW5zaXRpb25zID8gXCJlbmFibGUtdHJhbnNpdGlvbnNcIiA6IFwiXCJ9IHR3LXdpZGdldCB0dy1iZy13aGl0ZSB0dy1yb3VuZGVkLWxnIHR3LXNoYWRvdy1zbSB0dy10ZXh0LWdyYXktODAwICR7ZGVuc2l0eSA9PT0gXCJjb21wYWN0XCIgPyBcInR3LWRlbnNlXCIgOiBcIlwifWB9IHN0eWxlPXt0aGVtZVN0eWxlfT5cbiAgICAgICAgICAgIHtzZWFyY2hhYmxlID8gKFxuICAgICAgICAgICAgICAgIDxkaXYgY2xhc3NOYW1lPVwiZ3JpZC1jb250cm9sc1wiPlxuICAgICAgICAgICAgICAgICAgICA8aW5wdXRcbiAgICAgICAgICAgICAgICAgICAgICAgIGNsYXNzTmFtZT1cInNlYXJjaC1pbnB1dCB0dy1pbnB1dFwiXG4gICAgICAgICAgICAgICAgICAgICAgICBwbGFjZWhvbGRlcj17c2VhcmNoUGxhY2Vob2xkZXIgfHwgXCJTZWFyY2guLi5cIn1cbiAgICAgICAgICAgICAgICAgICAgICAgIHZhbHVlPXtzZWFyY2h9XG4gICAgICAgICAgICAgICAgICAgICAgICBvbkNoYW5nZT17KGUpID0+IHNldFNlYXJjaChlLnRhcmdldC52YWx1ZSl9XG4gICAgICAgICAgICAgICAgICAgIC8+XG4gICAgICAgICAgICAgICAgPC9kaXY+XG4gICAgICAgICAgICApIDogbnVsbH1cblxuICAgICAgICAgICAgPGRpdiBjbGFzc05hbWU9XCJncmlkLWFjdGlvbi1yb3dcIj5cbiAgICAgICAgICAgICAgICB7ZW5hYmxlQ3N2RXhwb3J0ID8gKFxuICAgICAgICAgICAgICAgICAgICA8YnV0dG9uIGNsYXNzTmFtZT1cImV4cG9ydC1idG4gdHctYnRuXCIgb25DbGljaz17KCkgPT4gZXhwb3J0VG9Dc3YoZXhwb3J0RmlsZU5hbWUgfHwgXCJleHBvcnRcIil9PkV4cG9ydCBDU1Y8L2J1dHRvbj5cbiAgICAgICAgICAgICAgICApIDogbnVsbH1cbiAgICAgICAgICAgIDwvZGl2PlxuXG4gICAgICAgICAgICA8ZGl2IGNsYXNzTmFtZT1cInRhYmxlLXdyYXBwZXJcIj5cbiAgICAgICAgICAgICAgICA8VGFibGVcbiAgICAgICAgICAgICAgICAgICAgY29sdW1ucz17dmlzaWJsZUNvbHVtbnN9XG4gICAgICAgICAgICAgICAgICAgIHJvd3M9e3BhZ2VSb3dzfVxuICAgICAgICAgICAgICAgICAgICBmb3JtYXRzPXtmb3JtYXRzfVxuICAgICAgICAgICAgICAgICAgICBhbGxvd1NvcnRpbmc9e2FsbG93U29ydGluZ31cbiAgICAgICAgICAgICAgICAgICAgc29ydFN0YXRlPXtzb3J0U3RhdGV9XG4gICAgICAgICAgICAgICAgICAgIG9uU29ydD17KHMpID0+IHNldFNvcnRTdGF0ZShzKX1cbiAgICAgICAgICAgICAgICAgICAgY29uc29saWRhdGU9e2NvbnNvbGlkYXRlfVxuICAgICAgICAgICAgICAgIC8+XG4gICAgICAgICAgICA8L2Rpdj5cblxuICAgICAgICAgICAge3Nob3dQYWdpbmF0aW9uID8gKFxuICAgICAgICAgICAgICAgIDxkaXYgY2xhc3NOYW1lPVwicGFnaW5hdGlvbi1jb250cm9sc1wiPlxuICAgICAgICAgICAgICAgICAgICA8YnV0dG9uIG9uQ2xpY2s9eygpID0+IHNldFBhZ2UoKHApID0+IE1hdGgubWF4KDEsIHAgLSAxKSl9IGRpc2FibGVkPXtwYWdlIDw9IDF9PiZsYXF1bzsgUHJldjwvYnV0dG9uPlxuICAgICAgICAgICAgICAgICAgICA8c3BhbiBjbGFzc05hbWU9XCJwYWdlLWluZm9cIj5QYWdlIHtwYWdlfSAvIHt0b3RhbFBhZ2VzfTwvc3Bhbj5cbiAgICAgICAgICAgICAgICAgICAgPGJ1dHRvbiBvbkNsaWNrPXsoKSA9PiBzZXRQYWdlKChwKSA9PiBNYXRoLm1pbih0b3RhbFBhZ2VzLCBwICsgMSkpfSBkaXNhYmxlZD17cGFnZSA+PSB0b3RhbFBhZ2VzfT5OZXh0ICZyYXF1bzs8L2J1dHRvbj5cbiAgICAgICAgICAgICAgICA8L2Rpdj5cbiAgICAgICAgICAgICkgOiBudWxsfVxuICAgICAgICA8L2Rpdj5cbiAgICApO1xufVxuIiwiaW1wb3J0IHsgY3JlYXRlRWxlbWVudCB9IGZyb20gXCJyZWFjdFwiO1xuXG5pbXBvcnQgeyBIZWxsb1dvcmxkU2FtcGxlIH0gZnJvbSBcIi4vY29tcG9uZW50cy9IZWxsb1dvcmxkU2FtcGxlXCI7XG5pbXBvcnQgXCIuL3VpL0R5bmFtaWNHcmlkLmNzc1wiO1xuXG5leHBvcnQgZnVuY3Rpb24gRHluYW1pY0dyaWQoeyBzYW1wbGVUZXh0LCBqc29uU3RyaW5nLCBhbGxvd1NvcnRpbmcsIHNlYXJjaGFibGUsIGNvbnNvbGlkYXRlLCBjb2x1bW5Gb3JtYXRzLCBzZWFyY2hQbGFjZWhvbGRlciwgc2hvd1BhZ2luYXRpb24sIHBhZ2VTaXplLCBlbmFibGVDc3ZFeHBvcnQsIGV4cG9ydEZpbGVOYW1lLCBkZW5zZSB9KSB7XG4gICAgY29uc3QgZGVuc2l0eSA9IGRlbnNlID8gXCJjb21wYWN0XCIgOiBcImNvbWZvcnRhYmxlXCI7XG4gICAgcmV0dXJuIChcbiAgICAgICAgPEhlbGxvV29ybGRTYW1wbGVcbiAgICAgICAgICAgIHNhbXBsZVRleHQ9e3NhbXBsZVRleHR9XG4gICAgICAgICAgICBqc29uU3RyaW5nPXtqc29uU3RyaW5nfVxuICAgICAgICAgICAgYWxsb3dTb3J0aW5nPXthbGxvd1NvcnRpbmd9XG4gICAgICAgICAgICBzZWFyY2hhYmxlPXtzZWFyY2hhYmxlfVxuICAgICAgICAgICAgY29uc29saWRhdGU9e2NvbnNvbGlkYXRlfVxuICAgICAgICAgICAgY29sdW1uRm9ybWF0cz17Y29sdW1uRm9ybWF0c31cbiAgICAgICAgICAgIHNlYXJjaFBsYWNlaG9sZGVyPXtzZWFyY2hQbGFjZWhvbGRlcn1cbiAgICAgICAgICAgIHNob3dQYWdpbmF0aW9uPXtzaG93UGFnaW5hdGlvbn1cbiAgICAgICAgICAgIHBhZ2VTaXplPXtwYWdlU2l6ZX1cbiAgICAgICAgICAgIGVuYWJsZUNzdkV4cG9ydD17ZW5hYmxlQ3N2RXhwb3J0fVxuICAgICAgICAgICAgZXhwb3J0RmlsZU5hbWU9e2V4cG9ydEZpbGVOYW1lfVxuICAgICAgICAgICAgZGVuc2l0eT17ZGVuc2l0eX1cbiAgICAgICAgLz5cbiAgICApO1xufVxuIl0sIm5hbWVzIjpbImZvcm1hdENlbGwiLCJ2YWx1ZSIsImZvcm1hdCIsIlN0cmluZyIsImYiLCJ0b0xvd2VyQ2FzZSIsIm51bSIsIk51bWJlciIsImlzTmFOIiwiSW50bCIsIk51bWJlckZvcm1hdCIsInVuZGVmaW5lZCIsInN0eWxlIiwiY3VycmVuY3kiLCJkIiwiRGF0ZSIsImdldFRpbWUiLCJ0b0xvY2FsZVN0cmluZyIsInRvVXBwZXJDYXNlIiwiVGFibGUiLCJjb2x1bW5zIiwicm93cyIsImZvcm1hdHMiLCJvblNvcnQiLCJzb3J0U3RhdGUiLCJhbGxvd1NvcnRpbmciLCJjb25zb2xpZGF0ZSIsImhhbmRsZUhlYWRlckNsaWNrIiwiY29sIiwiY29sdW1uIiwiZGlyZWN0aW9uIiwibmV4dERpciIsImNyZWF0ZUVsZW1lbnQiLCJjbGFzc05hbWUiLCJyb2xlIiwibWFwIiwia2V5Iiwib25DbGljayIsIm4iLCJsZW5ndGgiLCJzcGFuc0J5Q29sIiwiZm9yRWFjaCIsInNwYW5zIiwiQXJyYXkiLCJmaWxsIiwiaSIsInN0YXJ0VmFsIiwic3BhbiIsImoiLCJrIiwicm93IiwicmkiLCJyYXciLCJyb3dTcGFuIiwiSGVsbG9Xb3JsZFNhbXBsZSIsInNhbXBsZVRleHQiLCJqc29uU3RyaW5nIiwic2VhcmNoYWJsZSIsImNvbHVtbkZvcm1hdHMiLCJzZWFyY2hQbGFjZWhvbGRlciIsInNob3dQYWdpbmF0aW9uIiwicGFnZVNpemUiLCJlbmFibGVDc3ZFeHBvcnQiLCJleHBvcnRGaWxlTmFtZSIsImFjY2VudENvbG9yIiwiYWNjZW50Q29sb3IyIiwiYmx1ckFtb3VudCIsInNvcnRlZENvbHVtblRpbnQiLCJlbmFibGVTb3J0VHJhbnNpdGlvbnMiLCJkZW5zaXR5IiwicGFyc2VkIiwicmF3SW5wdXQiLCJpc0FycmF5IiwiY2FuZGlkYXRlS2V5cyIsIk9iamVjdCIsInByb3RvdHlwZSIsImhhc093blByb3BlcnR5IiwiY2FsbCIsInYiLCJKU09OIiwicGFyc2UiLCJlcnIiLCJyb3dzTWFwIiwiaXRlbSIsIkNvbHVtbiIsInZhbCIsIlZhbHVlIiwicm93S2V5IiwiUm93Iiwicm93S2V5cyIsImtleXMiLCJzb3J0IiwiYSIsImIiLCJlIiwiZGVyaXZlZENvbHVtbnMiLCJ1c2VNZW1vIiwidmlzaWJsZUNvbHVtbnMiLCJzZWFyY2giLCJzZXRTZWFyY2giLCJ1c2VTdGF0ZSIsImxvd2VyU2VhcmNoIiwiZmlsdGVyZWRSb3dzIiwiZmlsdGVyIiwiciIsInNvbWUiLCJjIiwiaW5jbHVkZXMiLCJzZXRTb3J0U3RhdGUiLCJzb3J0ZWRSb3dzIiwiZGlyIiwicm93c0NvcHkiLCJ2YSIsInZiIiwibmEiLCJuYiIsImxvY2FsZUNvbXBhcmUiLCJlZmZlY3RpdmVQYWdlU2l6ZSIsInBhZ2UiLCJzZXRQYWdlIiwidG90YWxSb3dzIiwidG90YWxQYWdlcyIsIk1hdGgiLCJtYXgiLCJjZWlsIiwicGFnZVN0YXJ0IiwicGFnZVJvd3MiLCJzbGljZSIsImV4cG9ydFRvQ3N2IiwiZmlsZU5hbWUiLCJyb3dzVG9FeHBvcnQiLCJjb2xzIiwiaGVhZGVyIiwiam9pbiIsImVzYyIsInMiLCJuZWVkc1dyYXAiLCJ0ZXN0IiwiZXNjYXBlZCIsInJlcGxhY2UiLCJsaW5lcyIsImNzdiIsImJsb2IiLCJCbG9iIiwidHlwZSIsInVybCIsIlVSTCIsImNyZWF0ZU9iamVjdFVSTCIsImRvY3VtZW50IiwiaHJlZiIsImRvd25sb2FkIiwiYm9keSIsImFwcGVuZENoaWxkIiwiY2xpY2siLCJyZW1vdmUiLCJyZXZva2VPYmplY3RVUkwiLCJ0aGVtZVN0eWxlIiwicGxhY2Vob2xkZXIiLCJvbkNoYW5nZSIsInRhcmdldCIsInAiLCJkaXNhYmxlZCIsIm1pbiIsIkR5bmFtaWNHcmlkIiwiZGVuc2UiXSwibWFwcGluZ3MiOiI7O0lBRUEsU0FBU0EsVUFBVUEsQ0FBQ0MsS0FBSyxFQUFFQyxNQUFNLEVBQUU7SUFDL0IsRUFBQSxJQUFJRCxLQUFLLElBQUksSUFBSSxFQUFFLE9BQU8sRUFBRSxDQUFBO0lBQzVCLEVBQUEsSUFBSSxDQUFDQyxNQUFNLEVBQUUsT0FBT0MsTUFBTSxDQUFDRixLQUFLLENBQUMsQ0FBQTtNQUVqQyxNQUFNRyxDQUFDLEdBQUdELE1BQU0sQ0FBQ0QsTUFBTSxDQUFDLENBQUNHLFdBQVcsRUFBRSxDQUFBO01BQ3RDLElBQUlELENBQUMsS0FBSyxVQUFVLEVBQUU7SUFDbEIsSUFBQSxNQUFNRSxHQUFHLEdBQUdDLE1BQU0sQ0FBQ04sS0FBSyxDQUFDLENBQUE7UUFDekIsSUFBSU0sTUFBTSxDQUFDQyxLQUFLLENBQUNGLEdBQUcsQ0FBQyxFQUFFLE9BQU9ILE1BQU0sQ0FBQ0YsS0FBSyxDQUFDLENBQUE7SUFDM0MsSUFBQSxPQUFPLElBQUlRLElBQUksQ0FBQ0MsWUFBWSxDQUFDQyxTQUFTLEVBQUU7SUFBRUMsTUFBQUEsS0FBSyxFQUFFLFVBQVU7SUFBRUMsTUFBQUEsUUFBUSxFQUFFLEtBQUE7SUFBTSxL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
