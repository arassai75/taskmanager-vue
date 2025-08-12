# TaskManager v1.1.0 Release Notes

**Release Date:** August 2025  
**Version:** 1.1.0

## What's New

- **Dark/Light Theme Toggle** - Switch between light and dark modes
- **Due Dates & Notifications** - Set task deadlines with browser alerts
- **Time Estimates** - Add hour estimates to tasks for better planning


## Technical Changes

### Backend (.NET)
- New database fields: dueDate, estimatedHours, notificationsEnabled
- Enhanced API endpoints support new fields
- Improved validation and error handling
- AutoMapper updates for new data

### Frontend (Vue.js)
- New form fields for due dates and estimates
- Theme toggle component
- Notification permission handling
- Enhanced task display with new information

### Database
- New columns in Tasks table
- Updated indexes for better performance
- Check constraints for data validation

## API Updates

**Tasks endpoints now support:**
- `dueDate` - Optional deadline
- `estimatedHours` - Optional time estimate
- `notificationsEnabled` - Enable/disable alerts

**New computed fields:**
- `isOverdue` - True if past due date
- `isDueSoon` - True if due within 24 hours
- `dueStatus` - Text status (Today, Tomorrow, Overdue, etc.)

## UI Improvements

- Date picker for due dates
- Number input for time estimates
- Toggle for notifications
- Due date badges with colors
- Time estimate display
- Overdue task highlighting
- Theme toggle in header
- Smooth color transitions

## Database Schema

**New Task Fields:**
- `DueDate` - DateTime, optional
- `EstimatedHours` - Decimal, optional
- `NotificationsEnabled` - Boolean, default true

**New Indexes:**
- Due date queries
- Time estimate filtering
- Notification status

## Setup Instructions

**Update Database:**
```bash
./deploy-db.sh
```

**Start Application:**
```bash
./quick-start.sh
```


