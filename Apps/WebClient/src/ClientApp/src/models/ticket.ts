import { TicketStatus } from "@/constants/ticketStatus";

export interface Ticket {
    id: string;
    room: string;
    nonce: string;
    createdTime: number;
    checkInAfter: number;
    tokenExpires: number;
    queuePosition: number;
    status: TicketStatus;
    token?: string;
}
