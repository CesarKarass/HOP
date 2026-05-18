// Servicio de chat y notificaciones
const API_BASE = '/api';

class ChatService {
    static async enviarMensaje(emisorId, receptorId, mensaje) {
        try {
            const response = await fetch(`${API_BASE}/chat/enviar`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ emisorId, receptorId, mensaje })
            });
            
            if (!response.ok) {
                throw new Error('Error al enviar mensaje');
            }
            
            return await response.text();
        } catch (error) {
            console.error('Error al enviar mensaje:', error);
            throw error;
        }
    }
    
    static async obtenerNotificaciones(usuarioId) {
        try {
            const response = await fetch(`${API_BASE}/chat/notificaciones/${usuarioId}`);
            
            if (!response.ok) {
                throw new Error('Error al obtener notificaciones');
            }
            
            return await response.json();
        } catch (error) {
            console.error('Error al obtener notificaciones:', error);
            return [];
        }
    }
}