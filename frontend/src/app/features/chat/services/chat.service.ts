import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class ChatService {

  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/chat`;

  streamAiResponse(message: string, conversationId?: string): Observable<{ chunk: string, messageId?: string }> {
    return new Observable(observer => {
      const controller = new AbortController();

      fetch(`${this.baseUrl}/stream`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'text/event-stream'
        },
        body: JSON.stringify({ message, conversationId }),
        signal: controller.signal
      }).then(response => {
        if (!response.body) {
          observer.error('No response body');
          return;
        }

        const reader = response.body.getReader();
        const decoder = new TextDecoder('utf-8');

        let receivedMessageId: string | undefined;

        const read = () => {
          reader.read().then(({ done, value }) => {
            if (done) {
              observer.complete();
              return;
            }

            const text = decoder.decode(value, { stream: true });

            // Jeśli to pierwsza linia: id:GUID\n\n
            if (text.startsWith('id:')) {
              receivedMessageId = text.replace('id:', '').trim();
              observer.next({ chunk: '', messageId: receivedMessageId });
            } else {
              observer.next({ chunk: text, messageId: receivedMessageId });
            }

            read();
          }).catch(error => observer.error(error));
        };

        read();
      }).catch(error => observer.error(error));

      return () => controller.abort();
    });
  }

  patchRating(messageId: string, rating: number): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/rate`, {
      messageId,
      rating
    });
  }

}
