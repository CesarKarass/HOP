// Servicio de gestión de servicios - ADAPTADO PARA TU API
const API_BASE = '/api';

class ServiciosService {
    static async buscar(filtros = {}) {
        try {
            // Tu backend espera un objeto JSON en el body para buscar
            const response = await fetch(`${API_BASE}/servicios/buscar`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    Busqueda: filtros.busqueda || '',
                    CategoriaId: filtros.categoriaId || null
                })
            });
            
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.error || 'Error al buscar servicios');
            }
            
            return await response.json();
        } catch (error) {
            console.error('Error en búsqueda:', error);
            return [];
        }
    }
    
    static async crear(titulo, usuarioId, ubicacion, categoriaId, descripcion) {
        try {
            const params = new URLSearchParams();
            params.append('titulo', titulo);
            params.append('usuarioId', usuarioId);
            params.append('ubicacion', ubicacion);
            params.append('categoriaId', categoriaId);
            params.append('descripcion', descripcion);
            
            const response = await fetch(`${API_BASE}/servicios`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: params
            });
            
            if (!response.ok) {
                throw new Error('Error al crear servicio');
            }
            
            return await response.text();
        } catch (error) {
            console.error('Error al crear servicio:', error);
            throw error;
        }
    }
    
    static async postularse(servicioId, prestadorId) {
        try {
            const params = new URLSearchParams();
            params.append('servicioId', servicioId);
            params.append('prestadorId', prestadorId);
            
            const response = await fetch(`${API_BASE}/servicios/postular`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: params
            });
            
            if (!response.ok) {
                const error = await response.text();
                throw new Error(error);
            }
            
            return await response.text();
        } catch (error) {
            console.error('Error al postularse:', error);
            throw error;
        }
    }
}