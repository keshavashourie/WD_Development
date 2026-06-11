import { createElement, useMemo, useState } from 'react';

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
  return createElement("table", {
    className: "dynamic-grid",
    role: "table"
  }, createElement("thead", null, createElement("tr", null, columns.map(col => createElement("th", {
    key: col,
    onClick: () => handleHeaderClick(col),
    className: allowSorting ? "sortable" : ""
  }, col, allowSorting && sortState && sortState.column === col ? sortState.direction === "asc" ? " \u25B2" : " \u25BC" : "")))), createElement("tbody", null, (() => {
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
    return rows.map((row, ri) => createElement("tr", {
      key: ri
    }, columns.map(col => {
      const span = spansByCol[col][ri];
      if (!span) return null; // covered by previous rowspan
      const raw = row[col];
      return createElement("td", {
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
    return createElement("div", {
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
    return createElement("div", {
      className: "widget-hello-world"
    }, "Invalid JSON provided");
  }
  if (!Array.isArray(parsed) || parsed.length === 0) {
    // support single object
    if (parsed && typeof parsed === "object") parsed = [parsed];else return createElement("div", {
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
  const derivedColumns = useMemo(() => Object.keys(parsed[0] || {}), [parsed]);

  // Visible columns (do not hide columns when consolidating; consolidation suppresses repeated cell values)
  const visibleColumns = useMemo(() => derivedColumns, [derivedColumns]);

  // Search state
  const [search, setSearch] = useState("");
  const lowerSearch = String(search || "").toLowerCase();
  const filteredRows = useMemo(() => {
    if (!searchable || !search) return parsed;
    return parsed.filter(r => {
      return visibleColumns.some(c => {
        const v = r[c];
        return v != null && String(v).toLowerCase().includes(lowerSearch);
      });
    });
  }, [parsed, search, searchable, visibleColumns, lowerSearch]);

  // Sorting state
  const [sortState, setSortState] = useState(null);
  const sortedRows = useMemo(() => {
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
  const [page, setPage] = useState(1);
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
  return createElement("div", {
    className: `widget-hello-world ${enableSortTransitions ? "enable-transitions" : ""} tw-widget tw-bg-white tw-rounded-lg tw-shadow-sm tw-text-gray-800 ${density === "compact" ? "tw-dense" : ""}`,
    style: themeStyle
  }, searchable ? createElement("div", {
    className: "grid-controls"
  }, createElement("input", {
    className: "search-input tw-input",
    placeholder: searchPlaceholder || "Search...",
    value: search,
    onChange: e => setSearch(e.target.value)
  })) : null, createElement("div", {
    className: "grid-action-row"
  }, enableCsvExport ? createElement("button", {
    className: "export-btn tw-btn",
    onClick: () => exportToCsv(exportFileName || "export")
  }, "Export CSV") : null), createElement("div", {
    className: "table-wrapper"
  }, createElement(Table, {
    columns: visibleColumns,
    rows: pageRows,
    formats: formats,
    allowSorting: allowSorting,
    sortState: sortState,
    onSort: s => setSortState(s),
    consolidate: consolidate
  })), showPagination ? createElement("div", {
    className: "pagination-controls"
  }, createElement("button", {
    onClick: () => setPage(p => Math.max(1, p - 1)),
    disabled: page <= 1
  }, "\xAB Prev"), createElement("span", {
    className: "page-info"
  }, "Page ", page, " / ", totalPages), createElement("button", {
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
  return createElement(HelloWorldSample, {
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

export { DynamicGrid };
//# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiRHluYW1pY0dyaWQubWpzIiwic291cmNlcyI6WyIuLi8uLi8uLi8uLi8uLi9zcmMvY29tcG9uZW50cy9IZWxsb1dvcmxkU2FtcGxlLmpzeCIsIi4uLy4uLy4uLy4uLy4uL3NyYy9EeW5hbWljR3JpZC5qc3giXSwic291cmNlc0NvbnRlbnQiOlsiaW1wb3J0IHsgY3JlYXRlRWxlbWVudCwgdXNlTWVtbywgdXNlU3RhdGUgfSBmcm9tIFwicmVhY3RcIjtcblxuZnVuY3Rpb24gZm9ybWF0Q2VsbCh2YWx1ZSwgZm9ybWF0KSB7XG4gICAgaWYgKHZhbHVlID09IG51bGwpIHJldHVybiBcIlwiO1xuICAgIGlmICghZm9ybWF0KSByZXR1cm4gU3RyaW5nKHZhbHVlKTtcblxuICAgIGNvbnN0IGYgPSBTdHJpbmcoZm9ybWF0KS50b0xvd2VyQ2FzZSgpO1xuICAgIGlmIChmID09PSBcImN1cnJlbmN5XCIpIHtcbiAgICAgICAgY29uc3QgbnVtID0gTnVtYmVyKHZhbHVlKTtcbiAgICAgICAgaWYgKE51bWJlci5pc05hTihudW0pKSByZXR1cm4gU3RyaW5nKHZhbHVlKTtcbiAgICAgICAgcmV0dXJuIG5ldyBJbnRsLk51bWJlckZvcm1hdCh1bmRlZmluZWQsIHsgc3R5bGU6IFwiY3VycmVuY3lcIiwgY3VycmVuY3k6IFwiVVNEXCIgfSkuZm9ybWF0KG51bSk7XG4gICAgfVxuICAgIGlmIChmID09PSBcIm51bWJlclwiKSB7XG4gICAgICAgIGNvbnN0IG51bSA9IE51bWJlcih2YWx1ZSk7XG4gICAgICAgIHJldHVybiBOdW1iZXIuaXNOYU4obnVtKSA/IFN0cmluZyh2YWx1ZSkgOiBTdHJpbmcobnVtKTtcbiAgICB9XG4gICAgaWYgKGYgPT09IFwiZGF0ZVwiKSB7XG4gICAgICAgIGNvbnN0IGQgPSBuZXcgRGF0ZSh2YWx1ZSk7XG4gICAgICAgIHJldHVybiBpc05hTihkLmdldFRpbWUoKSkgPyBTdHJpbmcodmFsdWUpIDogZC50b0xvY2FsZVN0cmluZygpO1xuICAgIH1cbiAgICBpZiAoZiA9PT0gXCJ1cHBlcmNhc2VcIikgcmV0dXJuIFN0cmluZyh2YWx1ZSkudG9VcHBlckNhc2UoKTtcbiAgICBpZiAoZiA9PT0gXCJsb3dlcmNhc2VcIikgcmV0dXJuIFN0cmluZyh2YWx1ZSkudG9Mb3dlckNhc2UoKTtcbiAgICByZXR1cm4gU3RyaW5nKHZhbHVlKTtcbn1cblxuZnVuY3Rpb24gVGFibGUoeyBjb2x1bW5zLCByb3dzLCBmb3JtYXRzLCBvblNvcnQsIHNvcnRTdGF0ZSwgYWxsb3dTb3J0aW5nLCBjb25zb2xpZGF0ZSB9KSB7XG4gICAgY29uc3QgaGFuZGxlSGVhZGVyQ2xpY2sgPSAoY29sKSA9PiB7XG4gICAgICAgIGlmICghYWxsb3dTb3J0aW5nKSByZXR1cm47XG4gICAgICAgIGlmICghb25Tb3J0KSByZXR1cm47XG4gICAgICAgIGNvbnN0IHsgY29sdW1uLCBkaXJlY3Rpb24gfSA9IHNvcnRTdGF0ZSB8fCB7fTtcbiAgICAgICAgbGV0IG5leHREaXIgPSBcImFzY1wiO1xuICAgICAgICBpZiAoY29sdW1uID09PSBjb2wgJiYgZGlyZWN0aW9uID09PSBcImFzY1wiKSBuZXh0RGlyID0gXCJkZXNjXCI7XG4gICAgICAgIG9uU29ydCh7IGNvbHVtbjogY29sLCBkaXJlY3Rpb246IG5leHREaXIgfSk7XG4gICAgfTtcblxuICAgIHJldHVybiAoXG4gICAgICAgIDx0YWJsZSBjbGFzc05hbWU9XCJkeW5hbWljLWdyaWRcIiByb2xlPVwidGFibGVcIj5cbiAgICAgICAgICAgIDx0aGVhZD5cbiAgICAgICAgICAgICAgICA8dHI+XG4gICAgICAgICAgICAgICAgICAgIHtjb2x1bW5zLm1hcCgoY29sKSA9PiAoXG4gICAgICAgICAgICAgICAgICAgICAgICA8dGgga2V5PXtjb2x9IG9uQ2xpY2s9eygpID0+IGhhbmRsZUhlYWRlckNsaWNrKGNvbCl9IGNsYXNzTmFtZT17YWxsb3dTb3J0aW5nID8gXCJzb3J0YWJsZVwiIDogXCJcIn0+XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAge2NvbH1cbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB7YWxsb3dTb3J0aW5nICYmIHNvcnRTdGF0ZSAmJiBzb3J0U3RhdGUuY29sdW1uID09PSBjb2wgPyAoc29ydFN0YXRlLmRpcmVjdGlvbiA9PT0gXCJhc2NcIiA/IFwiIFxcdTI1QjJcIiA6IFwiIFxcdTI1QkNcIikgOiBcIlwifVxuICAgICAgICAgICAgICAgICAgICAgICAgPC90aD5cbiAgICAgICAgICAgICAgICAgICAgKSl9XG4gICAgICAgICAgICAgICAgPC90cj5cbiAgICAgICAgICAgIDwvdGhlYWQ+XG4gICAgICAgICAgICA8dGJvZHk+XG4gICAgICAgICAgICAgICAgeygoKSA9PiB7XG4gICAgICAgICAgICAgICAgICAgIC8vIElmIGNvbnNvbGlkYXRpb24gd2l0aCByb3dzcGFuIGlzIHJlcXVlc3RlZCwgY29tcHV0ZSBzcGFucyBwZXIgY29sdW1uXG4gICAgICAgICAgICAgICAgICAgIGNvbnN0IG4gPSByb3dzLmxlbmd0aDtcbiAgICAgICAgICAgICAgICAgICAgY29uc3Qgc3BhbnNCeUNvbCA9IHt9O1xuICAgICAgICAgICAgICAgICAgICBjb2x1bW5zLmZvckVhY2goKGNvbCkgPT4ge1xuICAgICAgICAgICAgICAgICAgICAgICAgY29uc3Qgc3BhbnMgPSBuZXcgQXJyYXkobikuZmlsbCgwKTtcbiAgICAgICAgICAgICAgICAgICAgICAgIGlmICghY29uc29saWRhdGUpIHtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBmb3IgKGxldCBpID0gMDsgaSA8IG47IGkrKykgc3BhbnNbaV0gPSAxO1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNwYW5zQnlDb2xbY29sXSA9IHNwYW5zO1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybjtcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgICAgICAgICAgIGxldCBpID0gMDtcbiAgICAgICAgICAgICAgICAgICAgICAgIHdoaWxlIChpIDwgbikge1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGNvbnN0IHN0YXJ0VmFsID0gcm93c1tpXVtjb2xdO1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGxldCBzcGFuID0gMTtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBsZXQgaiA9IGkgKyAxO1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHdoaWxlIChqIDwgbiAmJiByb3dzW2pdW2NvbF0gPT09IHN0YXJ0VmFsKSB7XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHNwYW4rKztcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaisrO1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBzcGFuc1tpXSA9IHNwYW47XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgZm9yIChsZXQgayA9IGkgKyAxOyBrIDwgaSArIHNwYW47IGsrKykgc3BhbnNba10gPSAwOyAvLyBjb3ZlcmVkIGJ5IHJvd3NwYW5cbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBpID0gajtcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgICAgICAgICAgIHNwYW5zQnlDb2xbY29sXSA9IHNwYW5zO1xuICAgICAgICAgICAgICAgICAgICB9KTtcblxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gcm93cy5tYXAoKHJvdywgcmkpID0+IChcbiAgICAgICAgICAgICAgICAgICAgICAgIDx0ciBrZXk9e3JpfT5cbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB7Y29sdW1ucy5tYXAoKGNvbCkgPT4ge1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBjb25zdCBzcGFuID0gc3BhbnNCeUNvbFtjb2xdW3JpXTtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgaWYgKCFzcGFuKSByZXR1cm4gbnVsbDsgLy8gY292ZXJlZCBieSBwcmV2aW91cyByb3dzcGFuXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGNvbnN0IHJhdyA9IHJvd1tjb2xdO1xuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICByZXR1cm4gKFxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPHRkIGtleT17Y29sfSByb3dTcGFuPXtzcGFuID4gMSA/IHNwYW4gOiB1bmRlZmluZWR9PlxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHtmb3JtYXRDZWxsKHJhdywgZm9ybWF0cyAmJiBmb3JtYXRzW2NvbF0pfVxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgPC90ZD5cbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgKTtcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KX1cbiAgICAgICAgICAgICAgICAgICAgICAgIDwvdHI+XG4gICAgICAgICAgICAgICAgICAgICkpO1xuICAgICAgICAgICAgICAgIH0pKCl9XG4gICAgICAgICAgICA8L3Rib2R5PlxuICAgICAgICA8L3RhYmxlPlxuICAgICk7XG59XG5cbmV4cG9ydCBmdW5jdGlvbiBIZWxsb1dvcmxkU2FtcGxlKHsgc2FtcGxlVGV4dCwganNvblN0cmluZywgYWxsb3dTb3J0aW5nID0gdHJ1ZSwgc2VhcmNoYWJsZSA9IHRydWUsIGNvbnNvbGlkYXRlID0gZmFsc2UsIGNvbHVtbkZvcm1hdHMsIHNlYXJjaFBsYWNlaG9sZGVyLCBzaG93UGFnaW5hdGlvbiA9IGZhbHNlLCBwYWdlU2l6ZSA9IDEwLCBlbmFibGVDc3ZFeHBvcnQgPSBmYWxzZSwgZXhwb3J0RmlsZU5hbWUgPSBcImV4cG9ydFwiLCBhY2NlbnRDb2xvciA9IFwiIzYwYTVmYVwiLCBhY2NlbnRDb2xvcjIgPSBcIiM2ZWU3YjdcIiwgYmx1ckFtb3VudCA9IDgsIHNvcnRlZENvbHVtblRpbnQgPSBcInJnYmEoOTYsMTY1LDI1MCwwLjA2KVwiLCBlbmFibGVTb3J0VHJhbnNpdGlvbnMgPSB0cnVlLCBkZW5zaXR5ID0gXCJjb21mb3J0YWJsZVwiIH0pIHtcbiAgICAvLyBGYWxsYmFjayBuby1kYXRhXG4gICAgaWYgKCFqc29uU3RyaW5nKSB7XG4gICAgICAgIHJldHVybiA8ZGl2IGNsYXNzTmFtZT1cIndpZGdldC1oZWxsby13b3JsZFwiPkhlbGxvIHtzYW1wbGVUZXh0fTwvZGl2PjtcbiAgICB9XG5cbiAgICAvLyBBY2NlcHQgZWl0aGVyIGEgcmF3IEpTT04gc3RyaW5nLCBhbiBhbHJlYWR5LXBhcnNlZCBhcnJheS9vYmplY3QsXG4gICAgLy8gb3IgYW4gb2JqZWN0IHRoYXQgY29udGFpbnMgYSBzdHJpbmcgYXR0cmlidXRlIChjb21tb24gaW4gTWVuZGl4IGNvbnRleHRzKS5cbiAgICBsZXQgcGFyc2VkO1xuICAgIGxldCByYXdJbnB1dCA9IGpzb25TdHJpbmc7XG4gICAgaWYgKHJhd0lucHV0ICYmIHR5cGVvZiByYXdJbnB1dCA9PT0gXCJvYmplY3RcIiAmJiAhQXJyYXkuaXNBcnJheShyYXdJbnB1dCkpIHtcbiAgICAgICAgLy8gbG9vayBmb3IgY29tbW9uIGF0dHJpYnV0ZSBrZXlzIHRoYXQgbWlnaHQgY29udGFpbiB0aGUgSlNPTiBzdHJpbmdcbiAgICAgICAgY29uc3QgY2FuZGlkYXRlS2V5cyA9IFtcImpzb25cIiwgXCJ2YWx1ZVwiLCBcImRhdGFcIiwgXCJ0ZXh0XCJdO1xuICAgICAgICBmb3IgKGNvbnN0IGsgb2YgY2FuZGlkYXRlS2V5cykge1xuICAgICAgICAgICAgaWYgKE9iamVjdC5wcm90b3R5cGUuaGFzT3duUHJvcGVydHkuY2FsbChyYXdJbnB1dCwgaykpIHtcbiAgICAgICAgICAgICAgICBjb25zdCB2ID0gcmF3SW5wdXRba107XG4gICAgICAgICAgICAgICAgaWYgKHR5cGVvZiB2ID09PSBcInN0cmluZ1wiKSB7XG4gICAgICAgICAgICAgICAgICAgIHJhd0lucHV0ID0gdjtcbiAgICAgICAgICAgICAgICAgICAgYnJlYWs7XG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgICAgIC8vIHNvbWUgTWVuZGl4IGF0dHJpYnV0ZSBvYmplY3RzIGNhbiBiZSB7IHZhbHVlOiBcIi4uLlwiIH1cbiAgICAgICAgICAgICAgICBpZiAodiAmJiB0eXBlb2YgdiA9PT0gXCJvYmplY3RcIiAmJiB0eXBlb2Ygdi52YWx1ZSA9PT0gXCJzdHJpbmdcIikge1xuICAgICAgICAgICAgICAgICAgICByYXdJbnB1dCA9IHYudmFsdWU7XG4gICAgICAgICAgICAgICAgICAgIGJyZWFrO1xuICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgIH1cbiAgICAgICAgfVxuICAgIH1cblxuICAgIHRyeSB7XG4gICAgICAgIHBhcnNlZCA9IHR5cGVvZiByYXdJbnB1dCA9PT0gXCJzdHJpbmdcIiA/IEpTT04ucGFyc2UocmF3SW5wdXQpIDogcmF3SW5wdXQ7XG4gICAgfSBjYXRjaCAoZXJyKSB7XG4gICAgICAgIHJldHVybiA8ZGl2IGNsYXNzTmFtZT1cIndpZGdldC1oZWxsby13b3JsZFwiPkludmFsaWQgSlNPTiBwcm92aWRlZDwvZGl2PjtcbiAgICB9XG5cbiAgICBpZiAoIUFycmF5LmlzQXJyYXkocGFyc2VkKSB8fCBwYXJzZWQubGVuZ3RoID09PSAwKSB7XG4gICAgICAgIC8vIHN1cHBvcnQgc2luZ2xlIG9iamVjdFxuICAgICAgICBpZiAocGFyc2VkICYmIHR5cGVvZiBwYXJzZWQgPT09IFwib2JqZWN0XCIpIHBhcnNlZCA9IFtwYXJzZWRdO1xuICAgICAgICBlbHNlIHJldHVybiA8ZGl2IGNsYXNzTmFtZT1cIndpZGdldC1oZWxsby13b3JsZFwiPk5vIHJvd3MgdG8gZGlzcGxheTwvZGl2PjtcbiAgICB9XG5cbiAgICAvLyBTdXBwb3J0IGtleS92YWx1ZSBwYWlyIGFycmF5czogW3sgQ29sdW1uOiAnY29sMScsIFZhbHVlOiAndjEnLCBSb3c/OiAwIH0sIC4uLl1cbiAgICBpZiAoQXJyYXkuaXNBcnJheShwYXJzZWQpICYmIHBhcnNlZC5sZW5ndGggPiAwICYmIHR5cGVvZiBwYXJzZWRbMF0gPT09IFwib2JqZWN0XCIgJiYgKE9iamVjdC5wcm90b3R5cGUuaGFzT3duUHJvcGVydHkuY2FsbChwYXJzZWRbMF0sIFwiQ29sdW1uXCIpICYmIE9iamVjdC5wcm90b3R5cGUuaGFzT3duUHJvcGVydHkuY2FsbChwYXJzZWRbMF0sIFwiVmFsdWVcIikpKSB7XG4gICAgICAgIGNvbnN0IGNvbHVtbnMgPSBbXTtcbiAgICAgICAgY29uc3Qgcm93c01hcCA9IHt9O1xuICAgICAgICBwYXJzZWQuZm9yRWFjaCgoaXRlbSkgPT4ge1xuICAgICAgICAgICAgY29uc3QgY29sID0gaXRlbS5Db2x1bW47XG4gICAgICAgICAgICBjb25zdCB2YWwgPSBpdGVtLlZhbHVlO1xuICAgICAgICAgICAgY29uc3Qgcm93S2V5ID0gaXRlbS5Sb3cgIT0gbnVsbCA/IFN0cmluZyhpdGVtLlJvdykgOiBcIjBcIjtcbiAgICAgICAgICAgIGlmICghY29sdW1ucy5pbmNsdWRlcyhjb2wpKSBjb2x1bW5zLnB1c2goY29sKTtcbiAgICAgICAgICAgIGlmICghcm93c01hcFtyb3dLZXldKSByb3dzTWFwW3Jvd0tleV0gPSB7fTtcbiAgICAgICAgICAgIHJvd3NNYXBbcm93S2V5XVtjb2xdID0gdmFsO1xuICAgICAgICB9KTtcbiAgICAgICAgY29uc3Qgcm93S2V5cyA9IE9iamVjdC5rZXlzKHJvd3NNYXApLnNvcnQoKGEsIGIpID0+IE51bWJlcihhKSAtIE51bWJlcihiKSk7XG4gICAgICAgIHBhcnNlZCA9IHJvd0tleXMubWFwKChrKSA9PiByb3dzTWFwW2tdKTtcbiAgICB9XG5cbiAgICAvLyBQYXJzZSBjb2x1bW4gZm9ybWF0cyBtYXBwaW5nIGlmIHByb3ZpZGVkXG4gICAgbGV0IGZvcm1hdHMgPSBudWxsO1xuICAgIGlmIChjb2x1bW5Gb3JtYXRzKSB7XG4gICAgICAgIHRyeSB7XG4gICAgICAgICAgICBmb3JtYXRzID0gdHlwZW9mIGNvbHVtbkZvcm1hdHMgPT09IFwic3RyaW5nXCIgPyBKU09OLnBhcnNlKGNvbHVtbkZvcm1hdHMpIDogY29sdW1uRm9ybWF0cztcbiAgICAgICAgfSBjYXRjaCAoZSkge1xuICAgICAgICAgICAgZm9ybWF0cyA9IG51bGw7XG4gICAgICAgIH1cbiAgICB9XG5cbiAgICAvLyBEZXJpdmUgY29sdW1ucyBmcm9tIGZpcnN0IHJvdyAoYXNzdW1lIGNvbnNpc3RlbnQga2V5cylcbiAgICBjb25zdCBkZXJpdmVkQ29sdW1ucyA9IHVzZU1lbW8oKCkgPT4gT2JqZWN0LmtleXMocGFyc2VkWzBdIHx8IHt9KSwgW3BhcnNlZF0pO1xuXG4gICAgLy8gVmlzaWJsZSBjb2x1bW5zIChkbyBub3QgaGlkZSBjb2x1bW5zIHdoZW4gY29uc29saWRhdGluZzsgY29uc29saWRhdGlvbiBzdXBwcmVzc2VzIHJlcGVhdGVkIGNlbGwgdmFsdWVzKVxuICAgIGNvbnN0IHZpc2libGVDb2x1bW5zID0gdXNlTWVtbygoKSA9PiBkZXJpdmVkQ29sdW1ucywgW2Rlcml2ZWRDb2x1bW5zXSk7XG5cbiAgICAvLyBTZWFyY2ggc3RhdGVcbiAgICBjb25zdCBbc2VhcmNoLCBzZXRTZWFyY2hdID0gdXNlU3RhdGUoXCJcIik7XG4gICAgY29uc3QgbG93ZXJTZWFyY2ggPSBTdHJpbmcoc2VhcmNoIHx8IFwiXCIpLnRvTG93ZXJDYXNlKCk7XG5cbiAgICBjb25zdCBmaWx0ZXJlZFJvd3MgPSB1c2VNZW1vKCgpID0+IHtcbiAgICAgICAgaWYgKCFzZWFyY2hhYmxlIHx8ICFzZWFyY2gpIHJldHVybiBwYXJzZWQ7XG4gICAgICAgIHJldHVybiBwYXJzZWQuZmlsdGVyKChyKSA9PiB7XG4gICAgICAgICAgICByZXR1cm4gdmlzaWJsZUNvbHVtbnMuc29tZSgoYykgPT4ge1xuICAgICAgICAgICAgICAgIGNvbnN0IHYgPSByW2NdO1xuICAgICAgICAgICAgICAgIHJldHVybiB2ICE9IG51bGwgJiYgU3RyaW5nKHYpLnRvTG93ZXJDYXNlKCkuaW5jbHVkZXMobG93ZXJTZWFyY2gpO1xuICAgICAgICAgICAgfSk7XG4gICAgICAgIH0pO1xuICAgIH0sIFtwYXJzZWQsIHNlYXJjaCwgc2VhcmNoYWJsZSwgdmlzaWJsZUNvbHVtbnMsIGxvd2VyU2VhcmNoXSk7XG5cbiAgICAvLyBTb3J0aW5nIHN0YXRlXG4gICAgY29uc3QgW3NvcnRTdGF0ZSwgc2V0U29ydFN0YXRlXSA9IHVzZVN0YXRlKG51bGwpO1xuICAgIGNvbnN0IHNvcnRlZFJvd3MgPSB1c2VNZW1vKCgpID0+IHtcbiAgICAgICAgaWYgKCFzb3J0U3RhdGUgfHwgIXNvcnRTdGF0ZS5jb2x1bW4pIHJldHVybiBmaWx0ZXJlZFJvd3M7XG4gICAgICAgIGNvbnN0IGNvbCA9IHNvcnRTdGF0ZS5jb2x1bW47XG4gICAgICAgIGNvbnN0IGRpciA9IHNvcnRTdGF0ZS5kaXJlY3Rpb24gPT09IFwiZGVzY1wiID8gLTEgOiAxO1xuICAgICAgICBjb25zdCByb3dzQ29weSA9IFsuLi5maWx0ZXJlZFJvd3NdO1xuICAgICAgICByb3dzQ29weS5zb3J0KChhLCBiKSA9PiB7XG4gICAgICAgICAgICBjb25zdCB2YSA9IGFbY29sXTtcbiAgICAgICAgICAgIGNvbnN0IHZiID0gYltjb2xdO1xuICAgICAgICAgICAgaWYgKHZhID09IG51bGwgJiYgdmIgPT0gbnVsbCkgcmV0dXJuIDA7XG4gICAgICAgICAgICBpZiAodmEgPT0gbnVsbCkgcmV0dXJuIC0xICogZGlyO1xuICAgICAgICAgICAgaWYgKHZiID09IG51bGwpIHJldHVybiAxICogZGlyO1xuICAgICAgICAgICAgLy8gbnVtZXJpYyBjb21wYXJlIGlmIGJvdGggbnVtYmVyc1xuICAgICAgICAgICAgY29uc3QgbmEgPSBOdW1iZXIodmEpO1xuICAgICAgICAgICAgY29uc3QgbmIgPSBOdW1iZXIodmIpO1xuICAgICAgICAgICAgaWYgKCFOdW1iZXIuaXNOYU4obmEpICYmICFOdW1iZXIuaXNOYU4obmIpKSByZXR1cm4gKG5hIC0gbmIpICogZGlyO1xuICAgICAgICAgICAgcmV0dXJuIFN0cmluZyh2YSkubG9jYWxlQ29tcGFyZShTdHJpbmcodmIpKSAqIGRpcjtcbiAgICAgICAgfSk7XG4gICAgICAgIHJldHVybiByb3dzQ29weTtcbiAgICB9LCBbZmlsdGVyZWRSb3dzLCBzb3J0U3RhdGVdKTtcblxuICAgIC8vIFBhZ2luYXRpb24gc2V0dGluZ3NcbiAgICBjb25zdCBlZmZlY3RpdmVQYWdlU2l6ZSA9IHBhZ2VTaXplICYmIE51bWJlcihwYWdlU2l6ZSkgPiAwID8gTnVtYmVyKHBhZ2VTaXplKSA6IDEwO1xuICAgIGNvbnN0IFtwYWdlLCBzZXRQYWdlXSA9IHVzZVN0YXRlKDEpO1xuXG4gICAgY29uc3QgdG90YWxSb3dzID0gc29ydGVkUm93cy5sZW5ndGg7XG4gICAgY29uc3QgdG90YWxQYWdlcyA9IE1hdGgubWF4KDEsIE1hdGguY2VpbCh0b3RhbFJvd3MgLyBlZmZlY3RpdmVQYWdlU2l6ZSkpO1xuXG4gICAgLy8gQ2xhbXAgcGFnZVxuICAgIGlmIChwYWdlID4gdG90YWxQYWdlcykgc2V0UGFnZSh0b3RhbFBhZ2VzKTtcblxuICAgIGNvbnN0IHBhZ2VTdGFydCA9IChwYWdlIC0gMSkgKiBlZmZlY3RpdmVQYWdlU2l6ZTtcbiAgICBjb25zdCBwYWdlUm93cyA9IHNob3dQYWdpbmF0aW9uID8gc29ydGVkUm93cy5zbGljZShwYWdlU3RhcnQsIHBhZ2VTdGFydCArIGVmZmVjdGl2ZVBhZ2VTaXplKSA6IHNvcnRlZFJvd3M7XG5cbiAgICAvLyBDU1YgZXhwb3J0IChleHBvcnRzIGZpbHRlcmVkK3NvcnRlZCByb3dzLCBub3QganVzdCBjdXJyZW50IHBhZ2UpXG4gICAgY29uc3QgZXhwb3J0VG9Dc3YgPSAoZmlsZU5hbWUpID0+IHtcbiAgICAgICAgY29uc3Qgcm93c1RvRXhwb3J0ID0gc29ydGVkUm93cztcbiAgICAgICAgaWYgKCFyb3dzVG9FeHBvcnQgfHwgcm93c1RvRXhwb3J0Lmxlbmd0aCA9PT0gMCkgcmV0dXJuO1xuICAgICAgICBjb25zdCBjb2xzID0gdmlzaWJsZUNvbHVtbnM7XG4gICAgICAgIGNvbnN0IGhlYWRlciA9IGNvbHMuam9pbihcIixcIik7XG4gICAgICAgIGNvbnN0IGVzYyA9ICh2KSA9PiB7XG4gICAgICAgICAgICBpZiAodiA9PSBudWxsKSByZXR1cm4gXCJcIjtcbiAgICAgICAgICAgIGNvbnN0IHMgPSBTdHJpbmcodik7XG4gICAgICAgICAgICAvLyBlc2NhcGUgZG91YmxlIHF1b3Rlcywgd3JhcCB3aXRoIHF1b3RlcyBpZiBjb250YWlucyBjb21tYS9xdW90ZS9uZXdsaW5lXG4gICAgICAgICAgICBjb25zdCBuZWVkc1dyYXAgPSAvW1wiLFxcbl0vLnRlc3Qocyk7XG4gICAgICAgICAgICBjb25zdCBlc2NhcGVkID0gcy5yZXBsYWNlKC9cIi9nLCAnXCJcIicpO1xuICAgICAgICAgICAgcmV0dXJuIG5lZWRzV3JhcCA/IGBcIiR7ZXNjYXBlZH1cImAgOiBlc2NhcGVkO1xuICAgICAgICB9O1xuICAgICAgICBjb25zdCBsaW5lcyA9IFtoZWFkZXIsIC4uLnJvd3NUb0V4cG9ydC5tYXAoKHIpID0+IGNvbHMubWFwKChjKSA9PiBlc2MoZm9ybWF0Q2VsbChyW2NdLCBmb3JtYXRzICYmIGZvcm1hdHNbY10pKSkuam9pbihcIixcIikpXTtcbiAgICAgICAgY29uc3QgY3N2ID0gbGluZXMuam9pbihcIlxcblwiKTtcbiAgICAgICAgY29uc3QgYmxvYiA9IG5ldyBCbG9iKFtjc3ZdLCB7IHR5cGU6IFwidGV4dC9jc3Y7Y2hhcnNldD11dGYtODtcIiB9KTtcbiAgICAgICAgY29uc3QgdXJsID0gVVJMLmNyZWF0ZU9iamVjdFVSTChibG9iKTtcbiAgICAgICAgY29uc3QgYSA9IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQoXCJhXCIpO1xuICAgICAgICBhLmhyZWYgPSB1cmw7XG4gICAgICAgIGEuZG93bmxvYWQgPSBgJHtmaWxlTmFtZSB8fCBcImV4cG9ydFwifS5jc3ZgO1xuICAgICAgICBkb2N1bWVudC5ib2R5LmFwcGVuZENoaWxkKGEpO1xuICAgICAgICBhLmNsaWNrKCk7XG4gICAgICAgIGEucmVtb3ZlKCk7XG4gICAgICAgIFVSTC5yZXZva2VPYmplY3RVUkwodXJsKTtcbiAgICB9O1xuXG4gICAgY29uc3QgdGhlbWVTdHlsZSA9IHtcbiAgICAgICAgW1wiLS1hY2NlbnQtMlwiXTogYWNjZW50Q29sb3IsXG4gICAgICAgIFtcIi0tYWNjZW50XCJdOiBhY2NlbnRDb2xvcjIsXG4gICAgICAgIFtcIi0tYmx1ci1hbW91bnRcIl06IGAke2JsdXJBbW91bnR9cHhgLFxuICAgICAgICBbXCItLXNvcnRlZC10aW50XCJdOiBzb3J0ZWRDb2x1bW5UaW50LFxuICAgICAgICBbXCItLXJvdy1wYWRkaW5nXCJdOiBkZW5zaXR5ID09PSBcImNvbXBhY3RcIiA/IFwiNnB4XCIgOiBcIjEycHhcIlxuICAgIH07XG5cbiAgICByZXR1cm4gKFxuICAgICAgICA8ZGl2IGNsYXNzTmFtZT17YHdpZGdldC1oZWxsby13b3JsZCAke2VuYWJsZVNvcnRUcmFuc2l0aW9ucyA/IFwiZW5hYmxlLXRyYW5zaXRpb25zXCIgOiBcIlwifSB0dy13aWRnZXQgdHctYmctd2hpdGUgdHctcm91bmRlZC1sZyB0dy1zaGFkb3ctc20gdHctdGV4dC1ncmF5LTgwMCAke2RlbnNpdHkgPT09IFwiY29tcGFjdFwiID8gXCJ0dy1kZW5zZVwiIDogXCJcIn1gfSBzdHlsZT17dGhlbWVTdHlsZX0+XG4gICAgICAgICAgICB7c2VhcmNoYWJsZSA/IChcbiAgICAgICAgICAgICAgICA8ZGl2IGNsYXNzTmFtZT1cImdyaWQtY29udHJvbHNcIj5cbiAgICAgICAgICAgICAgICAgICAgPGlucHV0XG4gICAgICAgICAgICAgICAgICAgICAgICBjbGFzc05hbWU9XCJzZWFyY2gtaW5wdXQgdHctaW5wdXRcIlxuICAgICAgICAgICAgICAgICAgICAgICAgcGxhY2Vob2xkZXI9e3NlYXJjaFBsYWNlaG9sZGVyIHx8IFwiU2VhcmNoLi4uXCJ9XG4gICAgICAgICAgICAgICAgICAgICAgICB2YWx1ZT17c2VhcmNofVxuICAgICAgICAgICAgICAgICAgICAgICAgb25DaGFuZ2U9eyhlKSA9PiBzZXRTZWFyY2goZS50YXJnZXQudmFsdWUpfVxuICAgICAgICAgICAgICAgICAgICAvPlxuICAgICAgICAgICAgICAgIDwvZGl2PlxuICAgICAgICAgICAgKSA6IG51bGx9XG5cbiAgICAgICAgICAgIDxkaXYgY2xhc3NOYW1lPVwiZ3JpZC1hY3Rpb24tcm93XCI+XG4gICAgICAgICAgICAgICAge2VuYWJsZUNzdkV4cG9ydCA/IChcbiAgICAgICAgICAgICAgICAgICAgPGJ1dHRvbiBjbGFzc05hbWU9XCJleHBvcnQtYnRuIHR3LWJ0blwiIG9uQ2xpY2s9eygpID0+IGV4cG9ydFRvQ3N2KGV4cG9ydEZpbGVOYW1lIHx8IFwiZXhwb3J0XCIpfT5FeHBvcnQgQ1NWPC9idXR0b24+XG4gICAgICAgICAgICAgICAgKSA6IG51bGx9XG4gICAgICAgICAgICA8L2Rpdj5cblxuICAgICAgICAgICAgPGRpdiBjbGFzc05hbWU9XCJ0YWJsZS13cmFwcGVyXCI+XG4gICAgICAgICAgICAgICAgPFRhYmxlXG4gICAgICAgICAgICAgICAgICAgIGNvbHVtbnM9e3Zpc2libGVDb2x1bW5zfVxuICAgICAgICAgICAgICAgICAgICByb3dzPXtwYWdlUm93c31cbiAgICAgICAgICAgICAgICAgICAgZm9ybWF0cz17Zm9ybWF0c31cbiAgICAgICAgICAgICAgICAgICAgYWxsb3dTb3J0aW5nPXthbGxvd1NvcnRpbmd9XG4gICAgICAgICAgICAgICAgICAgIHNvcnRTdGF0ZT17c29ydFN0YXRlfVxuICAgICAgICAgICAgICAgICAgICBvblNvcnQ9eyhzKSA9PiBzZXRTb3J0U3RhdGUocyl9XG4gICAgICAgICAgICAgICAgICAgIGNvbnNvbGlkYXRlPXtjb25zb2xpZGF0ZX1cbiAgICAgICAgICAgICAgICAvPlxuICAgICAgICAgICAgPC9kaXY+XG5cbiAgICAgICAgICAgIHtzaG93UGFnaW5hdGlvbiA/IChcbiAgICAgICAgICAgICAgICA8ZGl2IGNsYXNzTmFtZT1cInBhZ2luYXRpb24tY29udHJvbHNcIj5cbiAgICAgICAgICAgICAgICAgICAgPGJ1dHRvbiBvbkNsaWNrPXsoKSA9PiBzZXRQYWdlKChwKSA9PiBNYXRoLm1heCgxLCBwIC0gMSkpfSBkaXNhYmxlZD17cGFnZSA8PSAxfT4mbGFxdW87IFByZXY8L2J1dHRvbj5cbiAgICAgICAgICAgICAgICAgICAgPHNwYW4gY2xhc3NOYW1lPVwicGFnZS1pbmZvXCI+UGFnZSB7cGFnZX0gLyB7dG90YWxQYWdlc308L3NwYW4+XG4gICAgICAgICAgICAgICAgICAgIDxidXR0b24gb25DbGljaz17KCkgPT4gc2V0UGFnZSgocCkgPT4gTWF0aC5taW4odG90YWxQYWdlcywgcCArIDEpKX0gZGlzYWJsZWQ9e3BhZ2UgPj0gdG90YWxQYWdlc30+TmV4dCAmcmFxdW87PC9idXR0b24+XG4gICAgICAgICAgICAgICAgPC9kaXY+XG4gICAgICAgICAgICApIDogbnVsbH1cbiAgICAgICAgPC9kaXY+XG4gICAgKTtcbn1cbiIsImltcG9ydCB7IGNyZWF0ZUVsZW1lbnQgfSBmcm9tIFwicmVhY3RcIjtcblxuaW1wb3J0IHsgSGVsbG9Xb3JsZFNhbXBsZSB9IGZyb20gXCIuL2NvbXBvbmVudHMvSGVsbG9Xb3JsZFNhbXBsZVwiO1xuaW1wb3J0IFwiLi91aS9EeW5hbWljR3JpZC5jc3NcIjtcblxuZXhwb3J0IGZ1bmN0aW9uIER5bmFtaWNHcmlkKHsgc2FtcGxlVGV4dCwganNvblN0cmluZywgYWxsb3dTb3J0aW5nLCBzZWFyY2hhYmxlLCBjb25zb2xpZGF0ZSwgY29sdW1uRm9ybWF0cywgc2VhcmNoUGxhY2Vob2xkZXIsIHNob3dQYWdpbmF0aW9uLCBwYWdlU2l6ZSwgZW5hYmxlQ3N2RXhwb3J0LCBleHBvcnRGaWxlTmFtZSwgZGVuc2UgfSkge1xuICAgIGNvbnN0IGRlbnNpdHkgPSBkZW5zZSA/IFwiY29tcGFjdFwiIDogXCJjb21mb3J0YWJsZVwiO1xuICAgIHJldHVybiAoXG4gICAgICAgIDxIZWxsb1dvcmxkU2FtcGxlXG4gICAgICAgICAgICBzYW1wbGVUZXh0PXtzYW1wbGVUZXh0fVxuICAgICAgICAgICAganNvblN0cmluZz17anNvblN0cmluZ31cbiAgICAgICAgICAgIGFsbG93U29ydGluZz17YWxsb3dTb3J0aW5nfVxuICAgICAgICAgICAgc2VhcmNoYWJsZT17c2VhcmNoYWJsZX1cbiAgICAgICAgICAgIGNvbnNvbGlkYXRlPXtjb25zb2xpZGF0ZX1cbiAgICAgICAgICAgIGNvbHVtbkZvcm1hdHM9e2NvbHVtbkZvcm1hdHN9XG4gICAgICAgICAgICBzZWFyY2hQbGFjZWhvbGRlcj17c2VhcmNoUGxhY2Vob2xkZXJ9XG4gICAgICAgICAgICBzaG93UGFnaW5hdGlvbj17c2hvd1BhZ2luYXRpb259XG4gICAgICAgICAgICBwYWdlU2l6ZT17cGFnZVNpemV9XG4gICAgICAgICAgICBlbmFibGVDc3ZFeHBvcnQ9e2VuYWJsZUNzdkV4cG9ydH1cbiAgICAgICAgICAgIGV4cG9ydEZpbGVOYW1lPXtleHBvcnRGaWxlTmFtZX1cbiAgICAgICAgICAgIGRlbnNpdHk9e2RlbnNpdHl9XG4gICAgICAgIC8+XG4gICAgKTtcbn1cbiJdLCJuYW1lcyI6WyJmb3JtYXRDZWxsIiwidmFsdWUiLCJmb3JtYXQiLCJTdHJpbmciLCJmIiwidG9Mb3dlckNhc2UiLCJudW0iLCJOdW1iZXIiLCJpc05hTiIsIkludGwiLCJOdW1iZXJGb3JtYXQiLCJ1bmRlZmluZWQiLCJzdHlsZSIsImN1cnJlbmN5IiwiZCIsIkRhdGUiLCJnZXRUaW1lIiwidG9Mb2NhbGVTdHJpbmciLCJ0b1VwcGVyQ2FzZSIsIlRhYmxlIiwiY29sdW1ucyIsInJvd3MiLCJmb3JtYXRzIiwib25Tb3J0Iiwic29ydFN0YXRlIiwiYWxsb3dTb3J0aW5nIiwiY29uc29saWRhdGUiLCJoYW5kbGVIZWFkZXJDbGljayIsImNvbCIsImNvbHVtbiIsImRpcmVjdGlvbiIsIm5leHREaXIiLCJjcmVhdGVFbGVtZW50IiwiY2xhc3NOYW1lIiwicm9sZSIsIm1hcCIsImtleSIsIm9uQ2xpY2siLCJuIiwibGVuZ3RoIiwic3BhbnNCeUNvbCIsImZvckVhY2giLCJzcGFucyIsIkFycmF5IiwiZmlsbCIsImkiLCJzdGFydFZhbCIsInNwYW4iLCJqIiwiayIsInJvdyIsInJpIiwicmF3Iiwicm93U3BhbiIsIkhlbGxvV29ybGRTYW1wbGUiLCJzYW1wbGVUZXh0IiwianNvblN0cmluZyIsInNlYXJjaGFibGUiLCJjb2x1bW5Gb3JtYXRzIiwic2VhcmNoUGxhY2Vob2xkZXIiLCJzaG93UGFnaW5hdGlvbiIsInBhZ2VTaXplIiwiZW5hYmxlQ3N2RXhwb3J0IiwiZXhwb3J0RmlsZU5hbWUiLCJhY2NlbnRDb2xvciIsImFjY2VudENvbG9yMiIsImJsdXJBbW91bnQiLCJzb3J0ZWRDb2x1bW5UaW50IiwiZW5hYmxlU29ydFRyYW5zaXRpb25zIiwiZGVuc2l0eSIsInBhcnNlZCIsInJhd0lucHV0IiwiaXNBcnJheSIsImNhbmRpZGF0ZUtleXMiLCJPYmplY3QiLCJwcm90b3R5cGUiLCJoYXNPd25Qcm9wZXJ0eSIsImNhbGwiLCJ2IiwiSlNPTiIsInBhcnNlIiwiZXJyIiwicm93c01hcCIsIml0ZW0iLCJDb2x1bW4iLCJ2YWwiLCJWYWx1ZSIsInJvd0tleSIsIlJvdyIsInJvd0tleXMiLCJrZXlzIiwic29ydCIsImEiLCJiIiwiZSIsImRlcml2ZWRDb2x1bW5zIiwidXNlTWVtbyIsInZpc2libGVDb2x1bW5zIiwic2VhcmNoIiwic2V0U2VhcmNoIiwidXNlU3RhdGUiLCJsb3dlclNlYXJjaCIsImZpbHRlcmVkUm93cyIsImZpbHRlciIsInIiLCJzb21lIiwiYyIsImluY2x1ZGVzIiwic2V0U29ydFN0YXRlIiwic29ydGVkUm93cyIsImRpciIsInJvd3NDb3B5IiwidmEiLCJ2YiIsIm5hIiwibmIiLCJsb2NhbGVDb21wYXJlIiwiZWZmZWN0aXZlUGFnZVNpemUiLCJwYWdlIiwic2V0UGFnZSIsInRvdGFsUm93cyIsInRvdGFsUGFnZXMiLCJNYXRoIiwibWF4IiwiY2VpbCIsInBhZ2VTdGFydCIsInBhZ2VSb3dzIiwic2xpY2UiLCJleHBvcnRUb0NzdiIsImZpbGVOYW1lIiwicm93c1RvRXhwb3J0IiwiY29scyIsImhlYWRlciIsImpvaW4iLCJlc2MiLCJzIiwibmVlZHNXcmFwIiwidGVzdCIsImVzY2FwZWQiLCJyZXBsYWNlIiwibGluZXMiLCJjc3YiLCJibG9iIiwiQmxvYiIsInR5cGUiLCJ1cmwiLCJVUkwiLCJjcmVhdGVPYmplY3RVUkwiLCJkb2N1bWVudCIsImhyZWYiLCJkb3dubG9hZCIsImJvZHkiLCJhcHBlbmRDaGlsZCIsImNsaWNrIiwicmVtb3ZlIiwicmV2b2tlT2JqZWN0VVJMIiwidGhlbWVTdHlsZSIsInBsYWNlaG9sZGVyIiwib25DaGFuZ2UiLCJ0YXJnZXQiLCJwIiwiZGlzYWJsZWQiLCJtaW4iLCJEeW5hbWljR3JpZCIsImRlbnNlIl0sIm1hcHBpbmdzIjoiOztBQUVBLFNBQVNBLFVBQVVBLENBQUNDLEtBQUssRUFBRUMsTUFBTSxFQUFFO0FBQy9CLEVBQUEsSUFBSUQsS0FBSyxJQUFJLElBQUksRUFBRSxPQUFPLEVBQUUsQ0FBQTtBQUM1QixFQUFBLElBQUksQ0FBQ0MsTUFBTSxFQUFFLE9BQU9DLE1BQU0sQ0FBQ0YsS0FBSyxDQUFDLENBQUE7RUFFakMsTUFBTUcsQ0FBQyxHQUFHRCxNQUFNLENBQUNELE1BQU0sQ0FBQyxDQUFDRyxXQUFXLEVBQUUsQ0FBQTtFQUN0QyxJQUFJRCxDQUFDLEtBQUssVUFBVSxFQUFFO0FBQ2xCLElBQUEsTUFBTUUsR0FBRyxHQUFHQyxNQUFNLENBQUNOLEtBQUssQ0FBQyxDQUFBO0lBQ3pCLElBQUlNLE1BQU0sQ0FBQ0MsS0FBSyxDQUFDRixHQUFHLENBQUMsRUFBRSxPQUFPSCxNQUFNLENBQUNGLEtBQUssQ0FBQyxDQUFBO0FBQzNDLElBQUEsT0FBTyxJQUFJUSxJQUFJLENBQUNDLFlBQVksQ0FBQ0MsU0FBUyxFQUFFO0FBQUVDLE1BQUFBLEtBQUssRUFBRSxVQUFVO0FBQUVDLE1BQUFBLFFBQVEsRUFBRSxLQUFBO0FBQU0sS0FBQyxDQUFDLENBQUNYLE1BQU0sQ0FBQ0ksR0FBRyxDQUFDLENBQUE7QUFDL0YsR0FBQTtFQUNBLElBQUlGLENBQUMsS0FBSyxRQUFRLEVBQUU7QUFDaEIsSUFBQSxNQUFNRSxHQUFHLEdBQUdDLE1BQU0sQ0FBQ04sS0FBSyxDQUFDLENBQUE7QUFDekIsSUFBQSxPQUFPTSxNQUFNLENBQUNDLEtBQUssQ0FBQ0YsR0FBRyxDQUFDLEdBQUdILE1BQU0sQ0FBQ0YsS0FBSyxDQUFDLEdBQUdFLE1BQU0sQ0FBQ0csR0FBRyxDQUFDLENBQUE7QUFDMUQsR0FBQTtFQUNBLElBQUlGLENBQUMsS0FBSyxNQUFNLEVBQUU7QUFDZCxJQUFBLE1BQU1VLENBQUMsR0FBRyxJQUFJQyxJQUFJLENBQUNkLEtBQUssQ0FBQyxDQUFBO0FBQ3pCLElBQUEsT0FBT08sS0FBSyxDQUFDTSxDQUFDLENBQUNFLE9BQU8sRUFBRSxDQUFDLEdBQUdiLE1BQU0sQ0FBQ0YsS0FBSyxDQUFDLEdBQUdhLENBQUMsQ0FBQ0csY0FBYyxFQUFFLENBQUE7QUFDbEUsR0FBQTtBQUNBLEVBQUEsSUFBSWIsQ0FBQyxLQUFLLFdBQVcsRUFBRSxPQUFPRCxNQUFNLENBQUNGLEtBQUssQ0FBQyxDQUFDaUIsV0FBVyxFQUFFLENBQUE7QUFDekQsRUFBQSxJQUFJZCxDQUFDLEtBQUssV0FBVyxFQUFFLE9BQU9ELE1BQU0sQ0FBQ0YsS0FBSyxDQUFDLENBQUNJLFdBQVcsRUFBRSxDQUFBO0VBQ3pELE9BQU9GLE1BQU0sQ0FBQ0YsS0FBSyxDQUFDLENBQUE7QUFDeEIsQ0FBQTtBQUVBLFNBQVNrQixLQUFLQSxDQUFDO0VBQUVDLE9BQU87RUFBRUMsSUFBSTtFQUFFQyxPQUFPO0VBQUVDLE1BQU07RUFBRUMsU0FBUztFQUFFQyxZQUFZO0FBQUVDLEVBQUFBLFdBQUFBO0FBQVksQ0FBQyxFQUFFO0VBQ3JGLE1BQU1DLGlCQUFpQixHQUFJQyxHQUFHLElBQUs7SUFDL0IsSUFBSSxDQUFDSCxZQUFZLEVBQUUsT0FBQTtJQUNuQixJQUFJLENBQUNGLE1BQU0sRUFBRSxPQUFBO0lBQ2IsTUFBTTtNQUFFTSxNQUFNO0FBQUVDLE1BQUFBLFNBQUFBO0FBQVUsS0FBQyxHQUFHTixTQUFTLElBQUksRUFBRSxDQUFBO0lBQzdDLElBQUlPLE9BQU8sR0FBRyxLQUFLLENBQUE7SUFDbkIsSUFBSUYsTUFBTSxLQUFLRCxHQUFHLElBQUlFLFNBQVMsS0FBSyxLQUFLLEVBQUVDLE9BQU8sR0FBRyxNQUFNLENBQUE7QUFDM0RSLElBQUFBLE1BQU0sQ0FBQztBQUFFTSxNQUFBQSxNQUFNLEVBQUVELEdBQUc7QUFBRUUsTUFBQUEsU0FBUyxFQUFFQyxPQUFBQTtBQUFRLEtBQUMsQ0FBQyxDQUFBO0dBQzlDLENBQUE7QUFFRCxFQUFBLE9BQ0lDLGFBQUEsQ0FBQSxPQUFBLEVBQUE7QUFBT0MsSUFBQUEsU0FBUyxFQUFDLGNBQWM7QUFBQ0MsSUFBQUEsSUFBSSxFQUFDLE9BQUE7R0FDakNGLEVBQUFBLGFBQUEsQ0FDSUEsT0FBQUEsRUFBQUEsSUFBQUEsRUFBQUEsYUFBQSxDQUNLWixJQUFBQSxFQUFBQSxJQUFBQSxFQUFBQSxPQUFPLENBQUNlLEdBQUcsQ0FBRVAsR0FBRyxJQUNiSSxhQUFBLENBQUEsSUFBQSxFQUFBO0FBQUlJLElBQUFBLEdBQUcsRUFBRVIsR0FBSTtBQUFDUyxJQUFBQSxPQUFPLEVBQUVBLE1BQU1WLGlCQUFpQixDQUFDQyxHQUFHLENBQUU7QUFBQ0ssSUFBQUEsU0FBUyxFQUFFUixZQUFZLEdBQUcsVUFBVSxHQUFHLEVBQUE7QUFBRyxHQUFBLEVBQzFGRyxHQUFHLEVBQ0hILFlBQVksSUFBSUQsU0FBUyxJQUFJQSxTQUFTLENBQUNLLE1BQU0sS0FBS0QsR0FBRyxHQUFJSixTQUFTLENBQUNNLFNBQVMsS0FBSyxLQUFLLEdBQUcsU0FBUyxHQUFHLFNBQVMsR0FBSSxFQUNuSCxDQUNQLENBQ0QsQ0FDRCxDQUFDLEVBQ1JFLGFBQUEsQ0FBQSxPQUFBLEVBQUEsSUFBQSxFQUNLLENBQUMsTUFBTTtBQUNKO0FBQ0EsSUFBQSxNQUFNTSxDQUFDLEdBQUdqQixJQUFJLENBQUNrQixNQUFNLENBQUE7SUFDckIsTUFBTUMsVUFBVSxHQUFHLEVBQUUsQ0FBQTtBQUNyQnBCLElBQUFBLE9BQU8sQ0FBQ3FCLE9BQU8sQ0FBRWIsR0FBRyxJQUFLO01BQ3JCLE1BQU1jLEtBQUssR0FBRyxJQUFJQyxLQUFLLENBQUNMLENBQUMsQ0FBQyxDQUFDTSxJQUFJLENBQUMsQ0FBQyxDQUFDLENBQUE7TUFDbEMsSUFBSSxDQUFDbEIsV0FBVyxFQUFFO0FBQ2QsUUFBQSxLQUFLLElBQUltQixDQUFDLEdBQUcsQ0FBQyxFQUFFQSxDQUFDLEdBQUdQLENBQUMsRUFBRU8sQ0FBQyxFQUFFLEVBQUVILEtBQUssQ0FBQ0csQ0FBQyxDQUFDLEdBQUcsQ0FBQyxDQUFBO0FBQ3hDTCxRQUFBQSxVQUFVLENBQUNaLEdBQUcsQ0FBQyxHQUFHYyxLQUFLLENBQUE7QUFDdkIsUUFBQSxPQUFBO0FBQ0osT0FBQTtNQUNBLElBQUlHLENBQUMsR0FBRyxDQUFDLENBQUE7TUFDVCxPQUFPQSxDQUFDLEdBQUdQLENBQUMsRUFBRTtRQUNWLE1BQU1RLFFBQVEsR0FBR3pCLElBQUksQ0FBQ3dCLENBQUMsQ0FBQyxDQUFDakIsR0FBRyxDQUFDLENBQUE7UUFDN0IsSUFBSW1CLElBQUksR0FBRyxDQUFDLENBQUE7QUFDWixRQUFBLElBQUlDLENBQUMsR0FBR0gsQ0FBQyxHQUFHLENBQUMsQ0FBQTtBQUNiLFFBQUEsT0FBT0csQ0FBQyxHQUFHVixDQUFDLElBQUlqQixJQUFJLENBQUMyQixDQUFDLENBQUMsQ0FBQ3BCLEdBQUcsQ0FBQyxLQUFLa0IsUUFBUSxFQUFFO0FBQ3ZDQyxVQUFBQSxJQUFJLEVBQUUsQ0FBQTtBQUNOQyxVQUFBQSxDQUFDLEVBQUUsQ0FBQTtBQUNQLFNBQUE7QUFDQU4sUUFBQUEsS0FBSyxDQUFDRyxDQUFDLENBQUMsR0FBR0UsSUFBSSxDQUFBO1FBQ2YsS0FBSyxJQUFJRSxDQUFDLEdBQUdKLENBQUMsR0FBRyxDQUFDLEVBQUVJLENBQUMsR0FBR0osQ0FBQyxHQUFHRSxJQUFJLEVBQUVFLENBQUMsRUFBRSxFQUFFUCxLQUFLLENBQUNPLENBQUMsQ0FBQyxHQUFHLENBQUMsQ0FBQztBQUNwREosUUFBQUEsQ0FBQyxHQUFHRyxDQUFDLENBQUE7QUFDVCxPQUFBO0FBQ0FSLE1BQUFBLFVBQVUsQ0FBQ1osR0FBRyxDQUFDLEdBQUdjLEtBQUssQ0FBQTtBQUMzQixLQUFDLENBQUMsQ0FBQTtJQUVGLE9BQU9yQixJQUFJLENBQUNjLEdBQUcsQ0FBQyxDQUFDZSxHQUFHLEVBQUVDLEVBQUUsS0FDcEJuQixhQUFBLENBQUEsSUFBQSxFQUFBO0FBQUlJLE1BQUFBLEdBQUcsRUFBRWUsRUFBQUE7QUFBRyxLQUFBLEVBQ1AvQixPQUFPLENBQUNlLEdBQUcsQ0FBRVAsR0FBRyxJQUFLO01BQ2xCLE1BQU1tQixJQUFJLEdBQUdQLFVBQVUsQ0FBQ1osR0FBRyxDQUFDLENBQUN1QixFQUFFLENBQUMsQ0FBQTtBQUNoQyxNQUFBLElBQUksQ0FBQ0osSUFBSSxFQUFFLE9BQU8sSUFBSSxDQUFDO0FBQ3ZCLE1BQUEsTUFBTUssR0FBRyxHQUFHRixHQUFHLENBQUN0QixHQUFHLENBQUMsQ0FBQTtBQUNwQixNQUFBLE9BQ0lJLGFBQUEsQ0FBQSxJQUFBLEVBQUE7QUFBSUksUUFBQUEsR0FBRyxFQUFFUixHQUFJO0FBQUN5QixRQUFBQSxPQUFPLEVBQUVOLElBQUksR0FBRyxDQUFDLEdBQUdBLElBQUksR0FBR3BDLFNBQUFBO09BQ3BDWCxFQUFBQSxVQUFVLENBQUNvRCxHQUFHLEVBQUU5QixPQUFPLElBQUlBLE9BQU8sQ0FBQ00sR0FBRyxDQUFDLENBQ3hDLENBQUMsQ0FBQTtLQUVaLENBQ0QsQ0FDUCxDQUFDLENBQUE7R0FDTCxHQUNFLENBQ0osQ0FBQyxDQUFBO0FBRWhCLENBQUE7QUFFTyxTQUFTMEIsZ0JBQWdCQSxDQUFDO0VBQUVDLFVBQVU7RUFBRUMsVUFBVTtBQUFFL0IsRUFBQUEsWUFBWSxHQUFHLElBQUk7QUFBRWdDLEVBQUFBLFVBQVUsR0FBRyxJQUFJO0FBQUUvQixFQUFBQSxXQUFXLEdBQUcsS0FBSztFQUFFZ0MsYUFBYTtFQUFFQyxpQkFBaUI7QUFBRUMsRUFBQUEsY0FBYyxHQUFHLEtBQUs7QUFBRUMsRUFBQUEsUUFBUSxHQUFHLEVBQUU7QUFBRUMsRUFBQUEsZUFBZSxHQUFHLEtBQUs7QUFBRUMsRUFBQUEsY0FBYyxHQUFHLFFBQVE7QUFBRUMsRUFBQUEsV0FBVyxHQUFHLFNBQVM7QUFBRUMsRUFBQUEsWUFBWSxHQUFHLFNBQVM7QUFBRUMsRUFBQUEsVUFBVSxHQUFHLENBQUM7QUFBRUMsRUFBQUEsZ0JBQWdCLEdBQUcsdUJBQXVCO0FBQUVDLEVBQUFBLHFCQUFxQixHQUFHLElBQUk7QUFBRUMsRUFBQUEsT0FBTyxHQUFHLGFBQUE7QUFBYyxDQUFDLEVBQUU7QUFDelo7RUFDQSxJQUFJLENBQUNiLFVBQVUsRUFBRTtBQUNiLElBQUEsT0FBT3hCLGFBQUEsQ0FBQSxLQUFBLEVBQUE7QUFBS0MsTUFBQUEsU0FBUyxFQUFDLG9CQUFBO0tBQXFCLEVBQUEsUUFBTSxFQUFDc0IsVUFBZ0IsQ0FBQyxDQUFBO0FBQ3ZFLEdBQUE7O0FBRUE7QUFDQTtBQUNBLEVBQUEsSUFBSWUsTUFBTSxDQUFBO0VBQ1YsSUFBSUMsUUFBUSxHQUFHZixVQUFVLENBQUE7QUFDekIsRUFBQSxJQUFJZSxRQUFRLElBQUksT0FBT0EsUUFBUSxLQUFLLFFBQVEsSUFBSSxDQUFDNUIsS0FBSyxDQUFDNkIsT0FBTyxDQUFDRCxRQUFRLENBQUMsRUFBRTtBQUN0RTtJQUNBLE1BQU1FLGFBQWEsR0FBRyxDQUFDLE1BQU0sRUFBRSxPQUFPLEVBQUUsTUFBTSxFQUFFLE1BQU0sQ0FBQyxDQUFBO0FBQ3ZELElBQUEsS0FBSyxNQUFNeEIsQ0FBQyxJQUFJd0IsYUFBYSxFQUFFO0FBQzNCLE1BQUEsSUFBSUMsTUFBTSxDQUFDQyxTQUFTLENBQUNDLGNBQWMsQ0FBQ0MsSUFBSSxDQUFDTixRQUFRLEVBQUV0QixDQUFDLENBQUMsRUFBRTtBQUNuRCxRQUFBLE1BQU02QixDQUFDLEdBQUdQLFFBQVEsQ0FBQ3RCLENBQUMsQ0FBQyxDQUFBO0FBQ3JCLFFBQUEsSUFBSSxPQUFPNkIsQ0FBQyxLQUFLLFFBQVEsRUFBRTtBQUN2QlAsVUFBQUEsUUFBUSxHQUFHTyxDQUFDLENBQUE7QUFDWixVQUFBLE1BQUE7QUFDSixTQUFBO0FBQ0E7QUFDQSxRQUFBLElBQUlBLENBQUMsSUFBSSxPQUFPQSxDQUFDLEtBQUssUUFBUSxJQUFJLE9BQU9BLENBQUMsQ0FBQzdFLEtBQUssS0FBSyxRQUFRLEVBQUU7VUFDM0RzRSxRQUFRLEdBQUdPLENBQUMsQ0FBQzdFLEtBQUssQ0FBQTtBQUNsQixVQUFBLE1BQUE7QUFDSixTQUFBO0FBQ0osT0FBQTtBQUNKLEtBQUE7QUFDSixHQUFBO0VBRUEsSUFBSTtBQUNBcUUsSUFBQUEsTUFBTSxHQUFHLE9BQU9DLFFBQVEsS0FBSyxRQUFRLEdBQUdRLElBQUksQ0FBQ0MsS0FBSyxDQUFDVCxRQUFRLENBQUMsR0FBR0EsUUFBUSxDQUFBO0dBQzFFLENBQUMsT0FBT1UsR0FBRyxFQUFFO0FBQ1YsSUFBQSxPQUFPakQsYUFBQSxDQUFBLEtBQUEsRUFBQTtBQUFLQyxNQUFBQSxTQUFTLEVBQUMsb0JBQUE7QUFBb0IsS0FBQSxFQUFDLHVCQUEwQixDQUFDLENBQUE7QUFDMUUsR0FBQTtBQUVBLEVBQUEsSUFBSSxDQUFDVSxLQUFLLENBQUM2QixPQUFPLENBQUNGLE1BQU0sQ0FBQyxJQUFJQSxNQUFNLENBQUMvQixNQUFNLEtBQUssQ0FBQyxFQUFFO0FBQy9DO0FBQ0EsSUFBQSxJQUFJK0IsTUFBTSxJQUFJLE9BQU9BLE1BQU0sS0FBSyxRQUFRLEVBQUVBLE1BQU0sR0FBRyxDQUFDQSxNQUFNLENBQUMsQ0FBQyxLQUN2RCxPQUFPdEMsYUFBQSxDQUFBLEtBQUEsRUFBQTtBQUFLQyxNQUFBQSxTQUFTLEVBQUMsb0JBQUE7QUFBb0IsS0FBQSxFQUFDLG9CQUF1QixDQUFDLENBQUE7QUFDNUUsR0FBQTs7QUFFQTtFQUNBLElBQUlVLEtBQUssQ0FBQzZCLE9BQU8sQ0FBQ0YsTUFBTSxDQUFDLElBQUlBLE1BQU0sQ0FBQy9CLE1BQU0sR0FBRyxDQUFDLElBQUksT0FBTytCLE1BQU0sQ0FBQyxDQUFDLENBQUMsS0FBSyxRQUFRLElBQUtJLE1BQU0sQ0FBQ0MsU0FBUyxDQUFDQyxjQUFjLENBQUNDLElBQUksQ0FBQ1AsTUFBTSxDQUFDLENBQUMsQ0FBQyxFQUFFLFFBQVEsQ0FBQyxJQUFJSSxNQUFNLENBQUNDLFNBQVMsQ0FBQ0MsY0FBYyxDQUFDQyxJQUFJLENBQUNQLE1BQU0sQ0FBQyxDQUFDLENBQUMsRUFBRSxPQUFPLENBQUUsRUFBRTtJQUV4TSxNQUFNWSxPQUFPLEdBQUcsRUFBRSxDQUFBO0FBQ2xCWixJQUFBQSxNQUFNLENBQUM3QixPQUFPLENBQUUwQyxJQUFJLElBQUs7QUFDckIsTUFBQSxNQUFNdkQsR0FBRyxHQUFHdUQsSUFBSSxDQUFDQyxNQUFNLENBQUE7QUFDdkIsTUFBQSxNQUFNQyxHQUFHLEdBQUdGLElBQUksQ0FBQ0csS0FBSyxDQUFBO0FBQ3RCLE1BQUEsTUFBTUMsTUFBTSxHQUFHSixJQUFJLENBQUNLLEdBQUcsSUFBSSxJQUFJLEdBQUdyRixNQUFNLENBQUNnRixJQUFJLENBQUNLLEdBQUcsQ0FBQyxHQUFHLEdBQUcsQ0FBQTtBQUV4RCxNQUFBLElBQUksQ0FBQ04sT0FBTyxDQUFDSyxNQUFNLENBQUMsRUFBRUwsT0FBTyxDQUFDSyxNQUFNLENBQUMsR0FBRyxFQUFFLENBQUE7QUFDMUNMLE1BQUFBLE9BQU8sQ0FBQ0ssTUFBTSxDQUFDLENBQUMzRCxHQUFHLENBQUMsR0FBR3lELEdBQUcsQ0FBQTtBQUM5QixLQUFDLENBQUMsQ0FBQTtJQUNGLE1BQU1JLE9BQU8sR0FBR2YsTUFBTSxDQUFDZ0IsSUFBSSxDQUFDUixPQUFPLENBQUMsQ0FBQ1MsSUFBSSxDQUFDLENBQUNDLENBQUMsRUFBRUMsQ0FBQyxLQUFLdEYsTUFBTSxDQUFDcUYsQ0FBQyxDQUFDLEdBQUdyRixNQUFNLENBQUNzRixDQUFDLENBQUMsQ0FBQyxDQUFBO0lBQzFFdkIsTUFBTSxHQUFHbUIsT0FBTyxDQUFDdEQsR0FBRyxDQUFFYyxDQUFDLElBQUtpQyxPQUFPLENBQUNqQyxDQUFDLENBQUMsQ0FBQyxDQUFBO0FBQzNDLEdBQUE7O0FBRUE7RUFDQSxJQUFJM0IsT0FBTyxHQUFHLElBQUksQ0FBQTtBQUNsQixFQUFBLElBQUlvQyxhQUFhLEVBQUU7SUFDZixJQUFJO0FBQ0FwQyxNQUFBQSxPQUFPLEdBQUcsT0FBT29DLGFBQWEsS0FBSyxRQUFRLEdBQUdxQixJQUFJLENBQUNDLEtBQUssQ0FBQ3RCLGFBQWEsQ0FBQyxHQUFHQSxhQUFhLENBQUE7S0FDMUYsQ0FBQyxPQUFPb0MsQ0FBQyxFQUFFO0FBQ1J4RSxNQUFBQSxPQUFPLEdBQUcsSUFBSSxDQUFBO0FBQ2xCLEtBQUE7QUFDSixHQUFBOztBQUVBO0VBQ0EsTUFBTXlFLGNBQWMsR0FBR0MsT0FBTyxDQUFDLE1BQU10QixNQUFNLENBQUNnQixJQUFJLENBQUNwQixNQUFNLENBQUMsQ0FBQyxDQUFDLElBQUksRUFBRSxDQUFDLEVBQUUsQ0FBQ0EsTUFBTSxDQUFDLENBQUMsQ0FBQTs7QUFFNUU7RUFDQSxNQUFNMkIsY0FBYyxHQUFHRCxPQUFPLENBQUMsTUFBTUQsY0FBYyxFQUFFLENBQUNBLGNBQWMsQ0FBQyxDQUFDLENBQUE7O0FBRXRFO0VBQ0EsTUFBTSxDQUFDRyxNQUFNLEVBQUVDLFNBQVMsQ0FBQyxHQUFHQyxRQUFRLENBQUMsRUFBRSxDQUFDLENBQUE7RUFDeEMsTUFBTUMsV0FBVyxHQUFHbEcsTUFBTSxDQUFDK0YsTUFBTSxJQUFJLEVBQUUsQ0FBQyxDQUFDN0YsV0FBVyxFQUFFLENBQUE7QUFFdEQsRUFBQSxNQUFNaUcsWUFBWSxHQUFHTixPQUFPLENBQUMsTUFBTTtBQUMvQixJQUFBLElBQUksQ0FBQ3ZDLFVBQVUsSUFBSSxDQUFDeUMsTUFBTSxFQUFFLE9BQU81QixNQUFNLENBQUE7QUFDekMsSUFBQSxPQUFPQSxNQUFNLENBQUNpQyxNQUFNLENBQUVDLENBQUMsSUFBSztBQUN4QixNQUFBLE9BQU9QLGNBQWMsQ0FBQ1EsSUFBSSxDQUFFQyxDQUFDLElBQUs7QUFDOUIsUUFBQSxNQUFNNUIsQ0FBQyxHQUFHMEIsQ0FBQyxDQUFDRSxDQUFDLENBQUMsQ0FBQTtBQUNkLFFBQUEsT0FBTzVCLENBQUMsSUFBSSxJQUFJLElBQUkzRSxNQUFNLENBQUMyRSxDQUFDLENBQUMsQ0FBQ3pFLFdBQVcsRUFBRSxDQUFDc0csUUFBUSxDQUFDTixXQUFXLENBQUMsQ0FBQTtBQUNyRSxPQUFDLENBQUMsQ0FBQTtBQUNOLEtBQUMsQ0FBQyxDQUFBO0FBQ04sR0FBQyxFQUFFLENBQUMvQixNQUFNLEVBQUU0QixNQUFNLEVBQUV6QyxVQUFVLEVBQUV3QyxjQUFjLEVBQUVJLFdBQVcsQ0FBQyxDQUFDLENBQUE7O0FBRTdEO0VBQ0EsTUFBTSxDQUFDN0UsU0FBUyxFQUFFb0YsWUFBWSxDQUFDLEdBQUdSLFFBQVEsQ0FBQyxJQUFJLENBQUMsQ0FBQTtBQUNoRCxFQUFBLE1BQU1TLFVBQVUsR0FBR2IsT0FBTyxDQUFDLE1BQU07SUFDN0IsSUFBSSxDQUFDeEUsU0FBUyxJQUFJLENBQUNBLFNBQVMsQ0FBQ0ssTUFBTSxFQUFFLE9BQU95RSxZQUFZLENBQUE7QUFDeEQsSUFBQSxNQUFNMUUsR0FBRyxHQUFHSixTQUFTLENBQUNLLE1BQU0sQ0FBQTtJQUM1QixNQUFNaUYsR0FBRyxHQUFHdEYsU0FBUyxDQUFDTSxTQUFTLEtBQUssTUFBTSxHQUFHLENBQUMsQ0FBQyxHQUFHLENBQUMsQ0FBQTtBQUNuRCxJQUFBLE1BQU1pRixRQUFRLEdBQUcsQ0FBQyxHQUFHVCxZQUFZLENBQUMsQ0FBQTtBQUNsQ1MsSUFBQUEsUUFBUSxDQUFDcEIsSUFBSSxDQUFDLENBQUNDLENBQUMsRUFBRUMsQ0FBQyxLQUFLO0FBQ3BCLE1BQUEsTUFBTW1CLEVBQUUsR0FBR3BCLENBQUMsQ0FBQ2hFLEdBQUcsQ0FBQyxDQUFBO0FBQ2pCLE1BQUEsTUFBTXFGLEVBQUUsR0FBR3BCLENBQUMsQ0FBQ2pFLEdBQUcsQ0FBQyxDQUFBO01BQ2pCLElBQUlvRixFQUFFLElBQUksSUFBSSxJQUFJQyxFQUFFLElBQUksSUFBSSxFQUFFLE9BQU8sQ0FBQyxDQUFBO01BQ3RDLElBQUlELEVBQUUsSUFBSSxJQUFJLEVBQUUsT0FBTyxDQUFDLENBQUMsR0FBR0YsR0FBRyxDQUFBO0FBQy9CLE1BQUEsSUFBSUcsRUFBRSxJQUFJLElBQUksRUFBRSxPQUFPLENBQUMsR0FBR0gsR0FBRyxDQUFBO0FBQzlCO0FBQ0EsTUFBQSxNQUFNSSxFQUFFLEdBQUczRyxNQUFNLENBQUN5RyxFQUFFLENBQUMsQ0FBQTtBQUNyQixNQUFBLE1BQU1HLEVBQUUsR0FBRzVHLE1BQU0sQ0FBQzBHLEVBQUUsQ0FBQyxDQUFBO01BQ3JCLElBQUksQ0FBQzFHLE1BQU0sQ0FBQ0MsS0FBSyxDQUFDMEcsRUFBRSxDQUFDLElBQUksQ0FBQzNHLE1BQU0sQ0FBQ0MsS0FBSyxDQUFDMkcsRUFBRSxDQUFDLEVBQUUsT0FBTyxDQUFDRCxFQUFFLEdBQUdDLEVBQUUsSUFBSUwsR0FBRyxDQUFBO0FBQ2xFLE1BQUEsT0FBTzNHLE1BQU0sQ0FBQzZHLEVBQUUsQ0FBQyxDQUFDSSxhQUFhLENBQUNqSCxNQUFNLENBQUM4RyxFQUFFLENBQUMsQ0FBQyxHQUFHSCxHQUFHLENBQUE7QUFDckQsS0FBQyxDQUFDLENBQUE7QUFDRixJQUFBLE9BQU9DLFFBQVEsQ0FBQTtBQUNuQixHQUFDLEVBQUUsQ0FBQ1QsWUFBWSxFQUFFOUUsU0FBUyxDQUFDLENBQUMsQ0FBQTs7QUFFN0I7QUFDQSxFQUFBLE1BQU02RixpQkFBaUIsR0FBR3hELFFBQVEsSUFBSXRELE1BQU0sQ0FBQ3NELFFBQVEsQ0FBQyxHQUFHLENBQUMsR0FBR3RELE1BQU0sQ0FBQ3NELFFBQVEsQ0FBQyxHQUFHLEVBQUUsQ0FBQTtFQUNsRixNQUFNLENBQUN5RCxJQUFJLEVBQUVDLE9BQU8sQ0FBQyxHQUFHbkIsUUFBUSxDQUFDLENBQUMsQ0FBQyxDQUFBO0FBRW5DLEVBQUEsTUFBTW9CLFNBQVMsR0FBR1gsVUFBVSxDQUFDdEUsTUFBTSxDQUFBO0FBQ25DLEVBQUEsTUFBTWtGLFVBQVUsR0FBR0MsSUFBSSxDQUFDQyxHQUFHLENBQUMsQ0FBQyxFQUFFRCxJQUFJLENBQUNFLElBQUksQ0FBQ0osU0FBUyxHQUFHSCxpQkFBaUIsQ0FBQyxDQUFDLENBQUE7O0FBRXhFO0FBQ0EsRUFBQSxJQUFJQyxJQUFJLEdBQUdHLFVBQVUsRUFBRUYsT0FBTyxDQUFDRSxVQUFVLENBQUMsQ0FBQTtBQUUxQyxFQUFBLE1BQU1JLFNBQVMsR0FBRyxDQUFDUCxJQUFJLEdBQUcsQ0FBQyxJQUFJRCxpQkFBaUIsQ0FBQTtBQUNoRCxFQUFBLE1BQU1TLFFBQVEsR0FBR2xFLGNBQWMsR0FBR2lELFVBQVUsQ0FBQ2tCLEtBQUssQ0FBQ0YsU0FBUyxFQUFFQSxTQUFTLEdBQUdSLGlCQUFpQixDQUFDLEdBQUdSLFVBQVUsQ0FBQTs7QUFFekc7RUFDQSxNQUFNbUIsV0FBVyxHQUFJQyxRQUFRLElBQUs7SUFDOUIsTUFBTUMsWUFBWSxHQUFHckIsVUFBVSxDQUFBO0lBQy9CLElBQUksQ0FBQ3FCLFlBQVksSUFBSUEsWUFBWSxDQUFDM0YsTUFBTSxLQUFLLENBQUMsRUFBRSxPQUFBO0lBQ2hELE1BQU00RixJQUFJLEdBQUdsQyxjQUFjLENBQUE7QUFDM0IsSUFBQSxNQUFNbUMsTUFBTSxHQUFHRCxJQUFJLENBQUNFLElBQUksQ0FBQyxHQUFHLENBQUMsQ0FBQTtJQUM3QixNQUFNQyxHQUFHLEdBQUl4RCxDQUFDLElBQUs7QUFDZixNQUFBLElBQUlBLENBQUMsSUFBSSxJQUFJLEVBQUUsT0FBTyxFQUFFLENBQUE7QUFDeEIsTUFBQSxNQUFNeUQsQ0FBQyxHQUFHcEksTUFBTSxDQUFDMkUsQ0FBQyxDQUFDLENBQUE7QUFDbkI7QUFDQSxNQUFBLE1BQU0wRCxTQUFTLEdBQUcsUUFBUSxDQUFDQyxJQUFJLENBQUNGLENBQUMsQ0FBQyxDQUFBO01BQ2xDLE1BQU1HLE9BQU8sR0FBR0gsQ0FBQyxDQUFDSSxPQUFPLENBQUMsSUFBSSxFQUFFLElBQUksQ0FBQyxDQUFBO0FBQ3JDLE1BQUEsT0FBT0gsU0FBUyxHQUFHLENBQUEsQ0FBQSxFQUFJRSxPQUFPLENBQUEsQ0FBQSxDQUFHLEdBQUdBLE9BQU8sQ0FBQTtLQUM5QyxDQUFBO0FBQ0QsSUFBQSxNQUFNRSxLQUFLLEdBQUcsQ0FBQ1IsTUFBTSxFQUFFLEdBQUdGLFlBQVksQ0FBQy9GLEdBQUcsQ0FBRXFFLENBQUMsSUFBSzJCLElBQUksQ0FBQ2hHLEdBQUcsQ0FBRXVFLENBQUMsSUFBSzRCLEdBQUcsQ0FBQ3RJLFVBQVUsQ0FBQ3dHLENBQUMsQ0FBQ0UsQ0FBQyxDQUFDLEVBQUVwRixPQUFPLElBQUlBLE9BQU8sQ0FBQ29GLENBQUMsQ0FBQyxDQUFDLENBQUMsQ0FBQyxDQUFDMkIsSUFBSSxDQUFDLEdBQUcsQ0FBQyxDQUFDLENBQUMsQ0FBQTtBQUMzSCxJQUFBLE1BQU1RLEdBQUcsR0FBR0QsS0FBSyxDQUFDUCxJQUFJLENBQUMsSUFBSSxDQUFDLENBQUE7SUFDNUIsTUFBTVMsSUFBSSxHQUFHLElBQUlDLElBQUksQ0FBQyxDQUFDRixHQUFHLENBQUMsRUFBRTtBQUFFRyxNQUFBQSxJQUFJLEVBQUUseUJBQUE7QUFBMEIsS0FBQyxDQUFDLENBQUE7QUFDakUsSUFBQSxNQUFNQyxHQUFHLEdBQUdDLEdBQUcsQ0FBQ0MsZUFBZSxDQUFDTCxJQUFJLENBQUMsQ0FBQTtBQUNyQyxJQUFBLE1BQU1sRCxDQUFDLEdBQUd3RCxRQUFRLENBQUNwSCxhQUFhLENBQUMsR0FBRyxDQUFDLENBQUE7SUFDckM0RCxDQUFDLENBQUN5RCxJQUFJLEdBQUdKLEdBQUcsQ0FBQTtBQUNackQsSUFBQUEsQ0FBQyxDQUFDMEQsUUFBUSxHQUFHLEdBQUdyQixRQUFRLElBQUksUUFBUSxDQUFNLElBQUEsQ0FBQSxDQUFBO0FBQzFDbUIsSUFBQUEsUUFBUSxDQUFDRyxJQUFJLENBQUNDLFdBQVcsQ0FBQzVELENBQUMsQ0FBQyxDQUFBO0lBQzVCQSxDQUFDLENBQUM2RCxLQUFLLEVBQUUsQ0FBQTtJQUNUN0QsQ0FBQyxDQUFDOEQsTUFBTSxFQUFFLENBQUE7QUFDVlIsSUFBQUEsR0FBRyxDQUFDUyxlQUFlLENBQUNWLEdBQUcsQ0FBQyxDQUFBO0dBQzNCLENBQUE7QUFFRCxFQUFBLE1BQU1XLFVBQVUsR0FBRztJQUNmLENBQUMsWUFBWSxHQUFHNUYsV0FBVztJQUMzQixDQUFDLFVBQVUsR0FBR0MsWUFBWTtBQUMxQixJQUFBLENBQUMsZUFBZSxHQUFHLENBQUdDLEVBQUFBLFVBQVUsQ0FBSSxFQUFBLENBQUE7SUFDcEMsQ0FBQyxlQUFlLEdBQUdDLGdCQUFnQjtBQUNuQyxJQUFBLENBQUMsZUFBZSxHQUFHRSxPQUFPLEtBQUssU0FBUyxHQUFHLEtBQUssR0FBRyxNQUFBO0dBQ3RELENBQUE7QUFFRCxFQUFBLE9BQ0lyQyxhQUFBLENBQUEsS0FBQSxFQUFBO0FBQUtDLElBQUFBLFNBQVMsRUFBRSxDQUFBLG1CQUFBLEVBQXNCbUMscUJBQXFCLEdBQUcsb0JBQW9CLEdBQUcsRUFBRSxDQUFzRUMsbUVBQUFBLEVBQUFBLE9BQU8sS0FBSyxTQUFTLEdBQUcsVUFBVSxHQUFHLEVBQUUsQ0FBRyxDQUFBO0FBQUN6RCxJQUFBQSxLQUFLLEVBQUVnSixVQUFBQTtHQUMxTW5HLEVBQUFBLFVBQVUsR0FDUHpCLGFBQUEsQ0FBQSxLQUFBLEVBQUE7QUFBS0MsSUFBQUEsU0FBUyxFQUFDLGVBQUE7QUFBZSxHQUFBLEVBQzFCRCxhQUFBLENBQUEsT0FBQSxFQUFBO0FBQ0lDLElBQUFBLFNBQVMsRUFBQyx1QkFBdUI7SUFDakM0SCxXQUFXLEVBQUVsRyxpQkFBaUIsSUFBSSxXQUFZO0FBQzlDMUQsSUFBQUEsS0FBSyxFQUFFaUcsTUFBTztJQUNkNEQsUUFBUSxFQUFHaEUsQ0FBQyxJQUFLSyxTQUFTLENBQUNMLENBQUMsQ0FBQ2lFLE1BQU0sQ0FBQzlKLEtBQUssQ0FBQTtBQUFFLEdBQzlDLENBQ0EsQ0FBQyxHQUNOLElBQUksRUFFUitCLGFBQUEsQ0FBQSxLQUFBLEVBQUE7QUFBS0MsSUFBQUEsU0FBUyxFQUFDLGlCQUFBO0dBQ1Y2QixFQUFBQSxlQUFlLEdBQ1o5QixhQUFBLENBQUEsUUFBQSxFQUFBO0FBQVFDLElBQUFBLFNBQVMsRUFBQyxtQkFBbUI7QUFBQ0ksSUFBQUEsT0FBTyxFQUFFQSxNQUFNMkYsV0FBVyxDQUFDakUsY0FBYyxJQUFJLFFBQVEsQ0FBQTtBQUFFLEdBQUEsRUFBQyxZQUFrQixDQUFDLEdBQ2pILElBQ0gsQ0FBQyxFQUVOL0IsYUFBQSxDQUFBLEtBQUEsRUFBQTtBQUFLQyxJQUFBQSxTQUFTLEVBQUMsZUFBQTtHQUNYRCxFQUFBQSxhQUFBLENBQUNiLEtBQUssRUFBQTtBQUNGQyxJQUFBQSxPQUFPLEVBQUU2RSxjQUFlO0FBQ3hCNUUsSUFBQUEsSUFBSSxFQUFFeUcsUUFBUztBQUNmeEcsSUFBQUEsT0FBTyxFQUFFQSxPQUFRO0FBQ2pCRyxJQUFBQSxZQUFZLEVBQUVBLFlBQWE7QUFDM0JELElBQUFBLFNBQVMsRUFBRUEsU0FBVTtBQUNyQkQsSUFBQUEsTUFBTSxFQUFHZ0gsQ0FBQyxJQUFLM0IsWUFBWSxDQUFDMkIsQ0FBQyxDQUFFO0FBQy9CN0csSUFBQUEsV0FBVyxFQUFFQSxXQUFBQTtBQUFZLEdBQzVCLENBQ0EsQ0FBQyxFQUVMa0MsY0FBYyxHQUNYNUIsYUFBQSxDQUFBLEtBQUEsRUFBQTtBQUFLQyxJQUFBQSxTQUFTLEVBQUMscUJBQUE7QUFBcUIsR0FBQSxFQUNoQ0QsYUFBQSxDQUFBLFFBQUEsRUFBQTtBQUFRSyxJQUFBQSxPQUFPLEVBQUVBLE1BQU1rRixPQUFPLENBQUV5QyxDQUFDLElBQUt0QyxJQUFJLENBQUNDLEdBQUcsQ0FBQyxDQUFDLEVBQUVxQyxDQUFDLEdBQUcsQ0FBQyxDQUFDLENBQUU7SUFBQ0MsUUFBUSxFQUFFM0MsSUFBSSxJQUFJLENBQUE7R0FBRyxFQUFBLFdBQW9CLENBQUMsRUFDckd0RixhQUFBLENBQUEsTUFBQSxFQUFBO0FBQU1DLElBQUFBLFNBQVMsRUFBQyxXQUFBO0dBQVksRUFBQSxPQUFLLEVBQUNxRixJQUFJLEVBQUMsS0FBRyxFQUFDRyxVQUFpQixDQUFDLEVBQzdEekYsYUFBQSxDQUFBLFFBQUEsRUFBQTtBQUFRSyxJQUFBQSxPQUFPLEVBQUVBLE1BQU1rRixPQUFPLENBQUV5QyxDQUFDLElBQUt0QyxJQUFJLENBQUN3QyxHQUFHLENBQUN6QyxVQUFVLEVBQUV1QyxDQUFDLEdBQUcsQ0FBQyxDQUFDLENBQUU7SUFBQ0MsUUFBUSxFQUFFM0MsSUFBSSxJQUFJRyxVQUFBQTtBQUFXLEdBQUEsRUFBQyxXQUFvQixDQUNySCxDQUFDLEdBQ04sSUFDSCxDQUFDLENBQUE7QUFFZDs7QUMvUk8sU0FBUzBDLFdBQVdBLENBQUM7RUFBRTVHLFVBQVU7RUFBRUMsVUFBVTtFQUFFL0IsWUFBWTtFQUFFZ0MsVUFBVTtFQUFFL0IsV0FBVztFQUFFZ0MsYUFBYTtFQUFFQyxpQkFBaUI7RUFBRUMsY0FBYztFQUFFQyxRQUFRO0VBQUVDLGVBQWU7RUFBRUMsY0FBYztBQUFFcUcsRUFBQUEsS0FBQUE7QUFBTSxDQUFDLEVBQUU7QUFDL0wsRUFBQSxNQUFNL0YsT0FBTyxHQUFHK0YsS0FBSyxHQUFHLFNBQVMsR0FBRyxhQUFhLENBQUE7RUFDakQsT0FDSXBJLGFBQUEsQ0FBQ3NCLGdCQUFnQixFQUFBO0FBQ2JDLElBQUFBLFVBQVUsRUFBRUEsVUFBVztBQUN2QkMsSUFBQUEsVUFBVSxFQUFFQSxVQUFXO0FBQ3ZCL0IsSUFBQUEsWUFBWSxFQUFFQSxZQUFhO0FBQzNCZ0MsSUFBQUEsVUFBVSxFQUFFQSxVQUFXO0FBQ3ZCL0IsSUFBQUEsV0FBVyxFQUFFQSxXQUFZO0FBQ3pCZ0MsSUFBQUEsYUFBYSxFQUFFQSxhQUFjO0FBQzdCQyxJQUFBQSxpQkFBaUIsRUFBRUEsaUJBQWtCO0FBQ3JDQyxJQUFBQSxjQUFjLEVBQUVBLGNBQWU7QUFDL0JDLElBQUFBLFFBQVEsRUFBRUEsUUFBUztBQUNuQkMsSUFBQUEsZUFBZSxFQUFFQSxlQUFnQjtBQUNqQ0MsSUFBQUEsY0FBYyxFQUFFQSxjQUFlO0FBQy9CTSxJQUFBQSxPQUFPLEVBQUVBLE9BQUFBO0FBQVEsR0FDcEIsQ0FBQyxDQUFBO0FBRVY7Ozs7In0=
