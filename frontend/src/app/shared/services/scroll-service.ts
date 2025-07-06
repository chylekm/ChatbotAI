import { ElementRef, Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ScrollService {

  constructor() { }

  public scrollToBottom(container: ElementRef<HTMLDivElement>): void {
    if (!this.shouldAutoScroll(container)) return;

    queueMicrotask(() => {
      container?.nativeElement.scrollTo({
        top: container.nativeElement.scrollHeight,
        behavior: 'smooth',
      });
    });
  }

  private shouldAutoScroll(container: ElementRef<HTMLDivElement>): boolean {
    const nativeContainer = container?.nativeElement;
    if (!nativeContainer) return false;

    const threshold = 400;
    const position = nativeContainer.scrollTop + nativeContainer.clientHeight;
    const height = nativeContainer.scrollHeight;

    return height - position <= threshold;
  }
}
