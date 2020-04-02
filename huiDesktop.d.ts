// API Version = 1

declare var huiDesktop: HuiDesktop;

declare class BasicWindow {
    get Left(): number;
    get Top(): number;
    set Left(value: number);
    set Top(value: number);
    set Width(value: number);
    set Height(value: number);
}

declare class WorkingArea {
    get Left(): number;
    get Top(): number;
    get Width(): number;
    get Height(): number;
}

declare class BasicScreen {
    get Width(): number;
    get Height(): number;
}

declare class HuiDesktop {
    get ApiVersion(): number;

    get Window(): BasicWindow;
    get WorkingArea(): WorkingArea;
    get Screen(): BasicScreen;

    set TopMost(value: boolean);
    set DragMoveLeft(value: boolean);
    set DragMoveRight(value: boolean);
    set ShowInTaskbar(value: boolean);
    set ClickTransparent(value: boolean);
}