import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ChatService {
  
  private readonly baseUrl = `${environment.apiUrl}/chat`;

  streamAiResponse(message: string, conversationId?: string): Observable<string> {
    return new Observable<string>((observer) => {
      const controller = new AbortController();

      fetch(`${this.baseUrl}/stream`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'text/event-stream'
        },
        body: JSON.stringify({ message, conversationId }),
        signal: controller.signal
      })
        .then(response => {
          if (!response.body) {
            observer.error('Response body is null');
            return;
          }

          const reader = response.body.getReader();
          const decoder = new TextDecoder();

          const read = () => {
            reader.read().then(({ done, value }) => {
              if (done) {
                observer.complete();
                return;
              }

              const chunk = decoder.decode(value, { stream: true });
              observer.next(chunk);
              read();
            }).catch(error => observer.error(error));
          };

          read();
        })
        .catch(error => observer.error(error));

      return () => controller.abort(); // obsługa anulowania
    });
  }
}
