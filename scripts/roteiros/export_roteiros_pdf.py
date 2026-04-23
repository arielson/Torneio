from __future__ import annotations

import html
import subprocess
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
DOCS_DIR = ROOT / "docs" / "roteiros"
TEMP_DIR = ROOT / "artifacts" / "roteiros-html"
DOWNLOADS_DIR = Path(r"C:\Users\ariel\Downloads")
CHROME = Path(r"C:\Program Files\Google\Chrome\Application\chrome.exe")

FILES = [
    ("01-admin-geral.md", "Roteiro_Admin_Geral.pdf"),
    ("02-admin-torneio.md", "Roteiro_Admin_Torneio.pdf"),
    ("03-fiscal-app.md", "Roteiro_Fiscal_App.pdf"),
    ("04-pescador.md", "Roteiro_Membro.pdf"),
    ("05-espectador.md", "Roteiro_Espectador.pdf"),
]


def markdown_to_html(markdown_text: str, title: str) -> str:
    lines = markdown_text.splitlines()
    html_lines: list[str] = []
    in_list = False

    def close_list() -> None:
        nonlocal in_list
        if in_list:
            html_lines.append("</ul>")
            in_list = False

    for raw in lines:
        line = raw.rstrip()
        stripped = line.strip()

        if not stripped:
            close_list()
            continue

        if stripped.startswith("# "):
            close_list()
            html_lines.append(f"<h1>{html.escape(stripped[2:])}</h1>")
            continue
        if stripped.startswith("## "):
            close_list()
            html_lines.append(f"<h2>{html.escape(stripped[3:])}</h2>")
            continue
        if stripped.startswith("### "):
            close_list()
            html_lines.append(f"<h3>{html.escape(stripped[4:])}</h3>")
            continue
        if stripped.startswith("- "):
            if not in_list:
                html_lines.append("<ul>")
                in_list = True
            html_lines.append(f"<li>{html.escape(stripped[2:])}</li>")
            continue
        if stripped[:2].isdigit() and stripped[1] == ".":
            close_list()
            html_lines.append(f"<p>{html.escape(stripped)}</p>")
            continue

        close_list()
        html_lines.append(f"<p>{html.escape(stripped)}</p>")

    close_list()

    body = "\n".join(html_lines)
    return f"""<!doctype html>
<html lang="pt-BR">
<head>
  <meta charset="utf-8">
  <title>{html.escape(title)}</title>
  <style>
    @page {{
      size: A4;
      margin: 18mm 16mm;
    }}
    body {{
      font-family: "Segoe UI", Arial, sans-serif;
      color: #111827;
      font-size: 11pt;
      line-height: 1.45;
    }}
    h1 {{
      font-size: 20pt;
      margin: 0 0 14px;
      color: #0f172a;
      border-bottom: 2px solid #cbd5e1;
      padding-bottom: 8px;
    }}
    h2 {{
      font-size: 14pt;
      margin: 18px 0 8px;
      color: #1d4ed8;
    }}
    h3 {{
      font-size: 12pt;
      margin: 14px 0 6px;
      color: #1f2937;
    }}
    p {{
      margin: 0 0 8px;
      text-align: justify;
    }}
    ul {{
      margin: 0 0 10px 18px;
      padding: 0;
    }}
    li {{
      margin: 0 0 5px;
    }}
  </style>
</head>
<body>
{body}
</body>
</html>"""


def export_pdf(md_name: str, pdf_name: str) -> None:
    md_path = DOCS_DIR / md_name
    html_path = TEMP_DIR / md_path.with_suffix(".html").name
    pdf_path = DOWNLOADS_DIR / pdf_name

    title = md_path.stem.replace("-", " ").title()
    html_content = markdown_to_html(md_path.read_text(encoding="utf-8"), title)
    html_path.write_text(html_content, encoding="utf-8")

    subprocess.run(
        [
            str(CHROME),
            "--headless=new",
            "--disable-gpu",
            f"--print-to-pdf={pdf_path}",
            html_path.resolve().as_uri(),
        ],
        check=True,
    )


def main() -> int:
    if not CHROME.exists():
        print(f"Chrome não encontrado em: {CHROME}", file=sys.stderr)
        return 1

    TEMP_DIR.mkdir(parents=True, exist_ok=True)
    DOWNLOADS_DIR.mkdir(parents=True, exist_ok=True)

    for md_name, pdf_name in FILES:
        export_pdf(md_name, pdf_name)
        print(f"Gerado: {DOWNLOADS_DIR / pdf_name}")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
