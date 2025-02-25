import typer
import os
from pathlib import Path
from typing import Annotated


def toPascalCase(name: str) -> str:
    # abc-def-ghi -> AbcDefGhi
    return "".join([word.capitalize() for word in name.split("-")])


def create_component(
    name: str,
    className: Annotated[str, typer.Option()] = None,
    subpath: Annotated[str, typer.Option()] = None,
    create_style: Annotated[bool, typer.Option("--style", "-s")] = False,
):
    component_path = Path(f"src/app/Controllers")
    views_path = Path(f"src/app/Views")
    styles_path = Path(f"src/app/Styles")
    if subpath:
        component_path /= subpath
        views_path /= subpath
        styles_path /= subpath

    component_path.mkdir(parents=True, exist_ok=True)
    views_path.mkdir(parents=True, exist_ok=True)
    styles_path.mkdir(parents=True, exist_ok=True)

    view_file_url = views_path / f"{name}.html"
    view_file_url_posix = view_file_url.relative_to(Path("src/app")).as_posix()
    style_file_url = styles_path / f"{name}.scss"
    style_file_url_posix = style_file_url.relative_to(Path("src/app")).as_posix()

    with open(component_path / f"{name}.component.ts", "w") as f:
        f.write(
            f"""import {{ Component }} from "@angular/core";

@Component({{
    selector: 'app-{name}',
    templateUrl: '{'../' * (len(component_path.parents) - 2)}{view_file_url_posix}',
    {f"styleUrls: ['{'../' * (len(component_path.parents) - 2)}{style_file_url_posix}']," if create_style else ""}
    standalone: false
}})
export class {className or toPascalCase(name)}Component {{
}}
"""
        )
        print(f"Component created at {component_path / f'{name}.component.ts'}")

    with open(view_file_url, "w") as f:
        f.write(f"""<!-- view for {name} -->""")
        print(f"View created at {view_file_url}")

    if create_style:
        with open(style_file_url, "w") as f:
            f.write(f"""/* style for {name} */""")
            print(f"Style created at {style_file_url}")

    print(f"Component {name} created successfully!")
    print("Please add it to app module manually.")


if __name__ == "__main__":
    typer.run(create_component)
