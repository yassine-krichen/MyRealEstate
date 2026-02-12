# Visitor Inquiry System Implementation Plan

## Overview

Creating a secure public-facing inquiry system for visitors without authentication.

## Security Model

- **Access Token**: 32-character random string generated per inquiry
- Visitors use token to access their inquiry: `/Inquiries/Track?token=ABC123...`
- No authentication required, but token acts as secure key

## Components

### 1. Public PropertiesController (`/Properties/*`)

- `Index` - List all published properties
- `Details/{id}` - View single property with inquiry form

### 2. Public InquiriesController (`/Inquiries/*`)

- `Create` - POST to create inquiry (from property or general)
- `Track?token=XXX` - View inquiry conversation by token
- `AddMessage` - POST to add visitor reply
- `MarkAnswered` - POST to mark as answered
- `Close` - POST to close inquiry

### 3. Update CreateInquiryCommand

- Generate 32-char access token on creation
- Store in Inquiry.AccessToken field

### 4. Views

- `/Views/Properties/Index.cshtml` - Property listing
- `/Views/Properties/Details.cshtml` - Property detail + inquiry form
- `/Views/Inquiries/Track.cshtml` - Inquiry tracking page
- `/Views/Inquiries/Created.cshtml` - Success page with tracking link

## User Flow

1. Visitor browses published properties
2. Clicks "Ask a Question" on property or uses general inquiry form
3. Submits inquiry with name, email, phone, message
4. Gets unique tracking link: `https://site.com/Inquiries/Track?token=ABC123...`
5. Can bookmark/save link to track responses
6. Agents reply via admin panel
7. Visitor sees replies on tracking page
8. Visitor can reply back or mark as answered/close

## Next: Implementation

Creating all files...
